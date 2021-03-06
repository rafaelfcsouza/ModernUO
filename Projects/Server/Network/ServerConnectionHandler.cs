/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019 - ModernUO Development Team                        *
 * Email: hi@modernuo.com                                                *
 * File: ServerConnectionHandler.cs                                      *
 * Created: 2020/04/12 - Updated: 2020/04/12                             *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * This program is distributed in the hope that it will be useful,       *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 * GNU General Public License for more details.                          *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Server.Network
{
  public class ServerConnectionHandler : ConnectionHandler
  {
    private readonly IMessagePumpService _messagePumpService;
    private readonly ILogger<ServerConnectionHandler> _logger;

    public ServerConnectionHandler(
      IMessagePumpService messagePumpService,
      ILogger<ServerConnectionHandler> logger
    )
    {
      _messagePumpService = messagePumpService;
      _logger = logger;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
      if (!VerifySocket(connection))
      {
        Release(connection);
        return;
      }

      NetState ns = new NetState(connection);
      TcpServer.Instances.Add(ns);
      _logger.LogInformation($"Client: {ns}: Connected. [{TcpServer.Instances.Count} Online]");

      connection.ConnectionClosed.Register(() => { TcpServer.Instances.Remove(ns); });

      await ProcessIncoming(ns);
    }

    private async Task ProcessIncoming(NetState ns)
    {
      var inPipe = ns.Connection.Transport.Input;

      while (true)
      {
        if (NetState.AsyncState.Paused)
          continue;

        try
        {
          ReadResult result = await inPipe.ReadAsync();
          if (result.IsCanceled || result.IsCompleted)
            return;

          ReadOnlySequence<byte> seq = result.Buffer;

          if (seq.IsEmpty)
            break;

          int pos = PacketHandlers.ProcessPacket(_messagePumpService, ns, seq);

          if (pos <= 0)
            break;

          inPipe.AdvanceTo(seq.Slice(0, pos).End);
        }
        catch
        {
          // ignored
        }
      }

      inPipe.Complete();
    }

    private static bool VerifySocket(ConnectionContext connection)
    {
      try
      {
        SocketConnectEventArgs args = new SocketConnectEventArgs(connection);

        EventSink.InvokeSocketConnect(args);

        return args.AllowConnection;
      }
      catch (Exception ex)
      {
        NetState.TraceException(ex);
        return false;
      }
    }

    private static void Release(ConnectionContext connection)
    {
      try
      {
        connection.Abort(new ConnectionAbortedException("Failed socket verification."));
      }
      catch (Exception ex)
      {
        NetState.TraceException(ex);
      }

      try
      {
        // TODO: Is this needed?
        connection.DisposeAsync();
      }
      catch (Exception ex)
      {
        NetState.TraceException(ex);
      }
    }
  }
}
