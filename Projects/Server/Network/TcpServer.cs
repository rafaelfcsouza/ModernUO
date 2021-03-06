/*************************************************************************
 * ModernUO                                                              *
 * Copyright (C) 2019 - ModernUO Development Team                        *
 * Email: hi@modernuo.com                                                *
 * File: TcpServer.cs                                                    *
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
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Network
{
  public class TcpServer
  {
    public static List<IPEndPoint> Listeners { get; } = new List<IPEndPoint>();
    // Make this thread safe
    public static List<NetState> Instances { get; } = new List<NetState>();

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
        .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")
        .ConfigureServices(services =>
        {
          services.AddSingleton<IMessagePumpService>(new MessagePumpService());
        })
        .UseKestrel(options =>
        {
          foreach (var ipep in Listeners)
          {
            options.Listen(ipep, builder => { builder.UseConnectionHandler<ServerConnectionHandler>(); });
            DisplayListener(ipep);
          }

          // Webservices here
        })
        .UseLibuv()
        .UseStartup<ServerStartup>();

    private static void DisplayListener(IPEndPoint ipep)
    {
      if (ipep.Address.Equals(IPAddress.Any) || ipep.Address.Equals(IPAddress.IPv6Any))
      {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adapter in adapters)
        {
          IPInterfaceProperties properties = adapter.GetIPProperties();
          foreach (UnicastIPAddressInformation unicast in properties.UnicastAddresses)
            if (ipep.AddressFamily == unicast.Address.AddressFamily)
              Console.WriteLine("Listening: {0}:{1}", unicast.Address, ipep.Port);
        }
      }
      else
        Console.WriteLine("Listening: {0}:{1}", ipep.Address, ipep.Port);
    }
  }
}
