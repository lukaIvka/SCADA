﻿using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

//Read za digitalne signale izlaza

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters parametri = this.CommandParameters as ModbusReadCommandParameters;
            byte[] paketi = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parametri.TransactionId)), 0, paketi, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parametri.ProtocolId)), 0,paketi, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parametri.Length)), 0, paketi, 4, 2);
            paketi[6] = parametri.UnitId;
            paketi[7] = parametri.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parametri.StartAddress)), 0, paketi, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)parametri.Quantity)), 0, paketi, 10, 2);
            return paketi;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters mpar = this.CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> dict = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort quantity = response[8]; //koliko bajtova treba citati; ByteCount - br BAJTOVA

            ushort value;
            Console.WriteLine(quantity);
            for (int i = 0; i < quantity; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    value = (ushort)(response[9 + i] & (byte)0x1);  //logicko i sa maskom cija je vrijednost 1
                    response[9 + i] /= 2;   //siftovanje

                    if (mpar.Quantity < (j + i * 8)) { break; } //Quantity je br BITOVA


                    dict.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(mpar.StartAddress + (j + i * 8))), value);
                }

            }
            return dict;
        }
    }
}