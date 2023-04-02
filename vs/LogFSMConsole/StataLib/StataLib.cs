/*
    This Library is to have an easy access to STAT DTA Files     
  
    Copyright (C) 2014  Konrad Mattheis (mattheis@ukma.de)
 
    This Software is available under the GPL and a comercial licence.
    For further information to the comercial licence please contact
    Konrad Mattheis. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

#region Usings
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using System.Collections;
using System.Runtime.InteropServices;
#endregion

namespace StataLib
{

    #region StataMap
    internal class StataMap
    {
        public UInt64 i01_stata_data { get; set; }
        public UInt64 i02_map { get; set; }
        public UInt64 i03_vartypes { get; set; }
        public UInt64 i04_varnames { get; set; }
        public UInt64 i05_sortlist { get; set; }
        public UInt64 i06_formats { get; set; }
        public UInt64 i07_value_label_names { get; set; }
        public UInt64 i08_variable_labels { get; set; }
        public UInt64 i09_characteristics { get; set; }
        public UInt64 i10_data { get; set; }
        public UInt64 i11_strls { get; set; }
        public UInt64 i12_value_labels { get; set; }
        public UInt64 i13_Stata_dataEnd { get; set; }
        public UInt64 i14_end_of_file { get; set; }

        public IEnumerable<UInt64> GetData()
        {
            yield return i01_stata_data;
            yield return i02_map;
            yield return i03_vartypes;
            yield return i04_varnames;
            yield return i05_sortlist;
            yield return i06_formats;
            yield return i07_value_label_names;
            yield return i08_variable_labels;
            yield return i09_characteristics;
            yield return i10_data;
            yield return i11_strls;
            yield return i12_value_labels;
            yield return i13_Stata_dataEnd;
            yield return i14_end_of_file;
        }
    }

    #endregion

    #region StataMissingValues
    public enum StataMissingValues
    {
        _empty = 0,
        a = 1,
        b = 2,
        c = 3,
        d = 4,
        e = 5,
        f = 6,
        g = 7,
        h = 8,
        i = 9,
        j = 10,
        k = 11,
        l = 12,
        m = 13,
        n = 14,
        o = 15,
        p = 16,
        q = 17,
        r = 18,
        s = 19,
        t = 20,
        u = 21,
        v = 22,
        w = 23,
        x = 24,
        y = 25,
        z = 26,
    }
    #endregion

    #region StataMissingValuesExtensionMethods
    public static class StataMissingValuesExtension
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct DoubleInt64
        {
            [FieldOffset(0)]
            public double Double;
            [FieldOffset(0)]
            public Int64 Int64;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct FloatInt32
        {
            [FieldOffset(0)]
            public float Float;
            [FieldOffset(0)]
            public Int32 Int32;
        }

        #region IsMissingValue
        public static bool IsMissingValue(this SByte data)
        {
            return data >= SByteMVStart;
        }

        public static bool IsMissingValue(this Int16 data)
        {
            return data >= Int16MVStart;
        }

        public static bool IsMissingValue(this Int32 data)
        {
            return data >= Int32MVStart;
        }

        public static bool IsMissingValue(this float data)
        {
            return data >= FloatMVStart;
        }

        public static bool IsMissingValue(this double data)
        {
            return data >= DoubleMVStart;
        }
        #endregion

        #region MissingValue
        public static StataMissingValues? MissingValue(this SByte data)
        {
            if (data.IsMissingValue())
            {
                return (StataMissingValues)(data - SByteMVStart);
            }
            else
                return null;
        }

        public static StataMissingValues? MissingValue(this Int16 data)
        {
            if (data.IsMissingValue())
            {
                return (StataMissingValues)(data - Int16MVStart);
            }
            else
                return null;
        }

        public static StataMissingValues? MissingValue(this Int32 data)
        {
            if (data.IsMissingValue())
            {
                return (StataMissingValues)(data - Int32MVStart);
            }
            else
                return null;
        }

        public static StataMissingValues? MissingValue(this float data)
        {
            if (data.IsMissingValue())
            {
                for (int i = FloatMV.Length - 1; i >= 0; i--)
                {
                    if (data >= FloatMV[i])
                    {
                        return (StataMissingValues)i;
                    }
                }
            }

            return null;
        }

        public static StataMissingValues? MissingValue(this double data)
        {
            if (data.IsMissingValue())
            {
                for (int i = FloatMV.Length - 1; i >= 0; i--)
                {
                    if (data >= DoubleMV[i])
                    {
                        return (StataMissingValues)i;
                    }
                }
            }

            return null;
        }
        #endregion

        #region GetMissingValue
        public static SByte GetMissingValueSByte(StataMissingValues mv)
        {
            return (SByte)(SByteMVStart + (SByte)(mv));
        }

        public static SByte GetMissingValue(this SByte data, StataMissingValues? mv)
        {
            if (mv.HasValue)
                return GetMissingValueSByte(mv.Value);
            else
                return data;
        }

        public static Int16 GetMissingValueInt16(StataMissingValues mv)
        {
            return (Int16)(Int16MVStart + (Int16)(mv));
        }

        public static Int16 GetMissingValue(this Int16 data, StataMissingValues? mv)
        {
            if (mv.HasValue)
                return GetMissingValueInt16(mv.Value);
            else
                return data;
        }

        public static Int32 GetMissingValueInt32(StataMissingValues mv)
        {
            return (Int32)(Int32MVStart + (Int32)(mv));
        }

        public static Int32 GetMissingValue(this Int32 data, StataMissingValues? mv)
        {
            if (mv.HasValue)
                return GetMissingValueInt32(mv.Value);
            else
                return data;
        }

        public static float GetMissingValueFloat(StataMissingValues mv)
        {
            return new FloatInt32() { Int32 = (0x7f000000 + (int)(mv) * 0x800) }.Float;
        }

        public static float GetMissingValue(this float data, StataMissingValues? mv)
        {
            if (mv.HasValue)
                return GetMissingValueFloat(mv.Value);
            else
                return data;
        }

        public static Double GetMissingValueDouble(StataMissingValues mv)
        {
            return new DoubleInt64() { Int64 = (0x7fe0000000000000 + (Int64)((Int32)(mv)) * 0x10000000000) }.Double;
        }

        public static Double GetMissingValue(this Double data, StataMissingValues? mv)
        {
            if (mv.HasValue)
                return GetMissingValueDouble(mv.Value);
            else
                return data;
        }
        #endregion

        #region Static Variables
        private static SByte SByteMVStart = SByte.MaxValue - 26; // 0x65;
        private static Int16 Int16MVStart = Int16.MaxValue - 26; // 0x7fe5;
        private static Int32 Int32MVStart = Int32.MaxValue - 26; // 0x7fffffe5;

        private static float FloatMVStart = GetMissingValueFloat(StataMissingValues._empty);
        private static double DoubleMVStart = GetMissingValueDouble(StataMissingValues._empty);

        private static float[] FloatMV = new float[] {
            GetMissingValueFloat(StataMissingValues._empty),
            GetMissingValueFloat(StataMissingValues.a),
            GetMissingValueFloat(StataMissingValues.b),
            GetMissingValueFloat(StataMissingValues.c),
            GetMissingValueFloat(StataMissingValues.d),
            GetMissingValueFloat(StataMissingValues.e),
            GetMissingValueFloat(StataMissingValues.f),
            GetMissingValueFloat(StataMissingValues.g),
            GetMissingValueFloat(StataMissingValues.h),
            GetMissingValueFloat(StataMissingValues.i),
            GetMissingValueFloat(StataMissingValues.j),
            GetMissingValueFloat(StataMissingValues.k),
            GetMissingValueFloat(StataMissingValues.l),
            GetMissingValueFloat(StataMissingValues.m),
            GetMissingValueFloat(StataMissingValues.n),
            GetMissingValueFloat(StataMissingValues.o),
            GetMissingValueFloat(StataMissingValues.p),
            GetMissingValueFloat(StataMissingValues.q),
            GetMissingValueFloat(StataMissingValues.r),
            GetMissingValueFloat(StataMissingValues.s),
            GetMissingValueFloat(StataMissingValues.t),
            GetMissingValueFloat(StataMissingValues.u),
            GetMissingValueFloat(StataMissingValues.v),
            GetMissingValueFloat(StataMissingValues.w),
            GetMissingValueFloat(StataMissingValues.x),
            GetMissingValueFloat(StataMissingValues.y),
            GetMissingValueFloat(StataMissingValues.z),
        };

        private static double[] DoubleMV = new double[] {
            GetMissingValueDouble(StataMissingValues._empty),
            GetMissingValueDouble(StataMissingValues.a),
            GetMissingValueDouble(StataMissingValues.b),
            GetMissingValueDouble(StataMissingValues.c),
            GetMissingValueDouble(StataMissingValues.d),
            GetMissingValueDouble(StataMissingValues.e),
            GetMissingValueDouble(StataMissingValues.f),
            GetMissingValueDouble(StataMissingValues.g),
            GetMissingValueDouble(StataMissingValues.h),
            GetMissingValueDouble(StataMissingValues.i),
            GetMissingValueDouble(StataMissingValues.j),
            GetMissingValueDouble(StataMissingValues.k),
            GetMissingValueDouble(StataMissingValues.l),
            GetMissingValueDouble(StataMissingValues.m),
            GetMissingValueDouble(StataMissingValues.n),
            GetMissingValueDouble(StataMissingValues.o),
            GetMissingValueDouble(StataMissingValues.p),
            GetMissingValueDouble(StataMissingValues.q),
            GetMissingValueDouble(StataMissingValues.r),
            GetMissingValueDouble(StataMissingValues.s),
            GetMissingValueDouble(StataMissingValues.t),
            GetMissingValueDouble(StataMissingValues.u),
            GetMissingValueDouble(StataMissingValues.v),
            GetMissingValueDouble(StataMissingValues.w),
            GetMissingValueDouble(StataMissingValues.x),
            GetMissingValueDouble(StataMissingValues.y),
            GetMissingValueDouble(StataMissingValues.z),
        };
        #endregion
    }
    #endregion

    #region StataVariable
    public class StataVariable
    {
        public enum StataVarType
        {
            FixedString = 0,
            String = 32768,
            Double = 65526,
            Float = 65527,
            Long = 65528,
            Int = 65529,
            Byte = 65530
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public StataVarType VarType { get; set; }

        public UInt16 FixedStringLength { get; set; }

        public string DisplayFormat { get; set; }

        public string ValueLabelName { get; set; }

        public StataVariable Clone()
        {
            return new StataVariable()
            {
                Description = Description,
                DisplayFormat = DisplayFormat,
                FixedStringLength = FixedStringLength,
                Name = Name,
                ValueLabelName = ValueLabelName,
                VarType = VarType,
            };
        }
    }
    #endregion

    #region StataBinaryWriter
    internal class StataBinaryWriter : BinaryWriter
    {
        private static Encoding stataEncoding = Encoding.GetEncoding(1252);

        public override void Write(string value)
        {
            byte[] data = stataEncoding.GetBytes(value);
            base.Write(data);
        }

        public StataBinaryWriter(Stream output)
            : base(output, Encoding.GetEncoding(1252))
        { }

        public void WriteGSO(UInt32 v, UInt32 o, string s)
        {
            Write("GSO");
            Write(v);
            Write(o);
            Write((byte)(130)); // TODO: TYPE ASCII -> Binary 129 Check if \0x00 contains 
            Write((UInt32)(s.Length + 1));
            Write(s);
            Write((byte)0x00);
        }
    }
    #endregion

    #region StataFileWriter
    public class StataFileWriter
    {
        #region Variables
        private StataMap Map { get; set; }

        private StataBinaryWriter bw;

        private Stream st;

        private Encoding env = Encoding.GetEncoding(1252);

        private IList<StataVariable> vars;

        private UInt32 dataCount = 0;

        private bool smallMemoryFootprint = false;

        private String tmpGSOfn;
        private Stream tmpGSOst;
        private StataBinaryWriter tmpGSObw;

        private Dictionary<UInt32, Dictionary<string, UInt32>> GSO = new Dictionary<UInt32, Dictionary<string, UInt32>>();

        private Dictionary<string, SortedDictionary<Int32, string>> ValueLabels = new Dictionary<string, SortedDictionary<Int32, string>>();

        private Encoding outputEncoding = Encoding.GetEncoding(1252);
        #endregion

        #region GetZeroedFixedString
        private string GetZeroedFixedString(string data, int len)
        {
            if (data == null)
                return "".PadRight(len + 1, (char)0x00);
            else
            {
                if (data.Length >= len)
                    data = data.Substring(1, len - 1);
                return data.PadRight(len + 1, (char)0x00);
            }
        }
        #endregion

        #region Constructor
        public StataFileWriter(Stream st, IList<StataVariable> vars, string DataName, bool SmallMemoryFootprint)
        {
            this.smallMemoryFootprint = SmallMemoryFootprint;
            if (smallMemoryFootprint)
            {
                try
                {
                    tmpGSOfn = System.IO.Path.GetTempFileName().Replace(".tmp", ".GSO");
                    tmpGSOst = File.Create(tmpGSOfn);
                    tmpGSObw = new StataBinaryWriter(tmpGSOst);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating temporary GSO-File for smallmemoryfootprint, try without enable smallmemoryfootprint: original Error" + ex.Message);
                }
            }

            this.st = st;
            this.vars = vars;
            if (!st.CanSeek)
                throw new ArgumentException("Base Stream has to support Seek");
            Map = new StataMap();
            bw = new StataBinaryWriter(st);
            Map.i01_stata_data = (ulong)st.Position;
            bw.Write("<stata_dta>");
            bw.Write("<header>");
            bw.Write("<release>117</release>");
            bw.Write("<byteorder>" + (BitConverter.IsLittleEndian ? "LSF" : "MSF") + "</byteorder>");
            bw.Write("<K>");
            bw.Write((UInt16)vars.Count);
            bw.Write("</K>");

            bw.Write("<N>");
            bw.Write((UInt32)0);
            // LATER Write here correct Number of Data
            bw.Write("</N>");


            if (DataName == null)
                DataName = "";
            else
                if (DataName.Length > 80)
                DataName = DataName.Substring(1, 80);

            bw.Write("<label>");
            bw.Write((byte)DataName.Length);
            bw.Write(DataName);
            bw.Write("</label>");

            bw.Write("<timestamp>" + (char)17 + DateTime.Now.ToString("dd MMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture) + "</timestamp>");

            bw.Write("</header>");

            Map.i02_map = (ulong)st.Position;
            bw.Write("<map>");
            foreach (var item in Map.GetData())
                bw.Write(item);
            bw.Write("</map>");

            Map.i03_vartypes = (ulong)st.Position;
            bw.Write("<variable_types>");
            foreach (var item in vars)
            {
                if (item.VarType == StataVariable.StataVarType.FixedString)
                {
                    // TODO: check fixed string length
                    bw.Write((UInt16)item.FixedStringLength);
                }
                else
                    bw.Write((UInt16)item.VarType);
            }
            bw.Write("</variable_types>");

            Map.i04_varnames = (ulong)st.Position;
            bw.Write("<varnames>");
            foreach (var item in vars)
            {
                bw.Write(GetZeroedFixedString(item.Name, 32));
            }
            bw.Write("</varnames>");

            Map.i05_sortlist = (ulong)st.Position;
            bw.Write("<sortlist>");
            foreach (var item in vars)
            {
                // TODO: add Sortlist Feature
                bw.Write((UInt16)0);
            }
            bw.Write((UInt16)0);
            bw.Write("</sortlist>");

            Map.i06_formats = (ulong)st.Position;
            bw.Write("<formats>");
            foreach (var item in vars)
            {
                bw.Write(GetZeroedFixedString(item.DisplayFormat, 48));
            }
            bw.Write("</formats>");

            Map.i07_value_label_names = (ulong)st.Position;
            bw.Write("<value_label_names>");
            foreach (var item in vars)
            {
                bw.Write(GetZeroedFixedString(item.ValueLabelName, 32));
            }
            bw.Write("</value_label_names>");

            Map.i08_variable_labels = (ulong)st.Position;
            bw.Write("<variable_labels>");
            foreach (var item in vars)
            {
                bw.Write(GetZeroedFixedString(item.Description, 80));
            }
            bw.Write("</variable_labels>");

            Map.i09_characteristics = (ulong)st.Position;
            bw.Write("<characteristics></characteristics>");

            Map.i10_data = (ulong)st.Position;
            bw.Write("<data>");
        }

        public StataFileWriter(String FileName, IList<StataVariable> vars, string DataName)
            : this(File.Create(FileName), vars, DataName, false)
        {
        }

        public StataFileWriter(String FileName, IList<StataVariable> vars, string DataName, bool SmallMemoryFootprint)
            : this(File.Create(FileName), vars, DataName, SmallMemoryFootprint)
        {
        }
        #endregion

        #region AddValueLabels
        public void AddValueLabel(string Name, StataMissingValues mv, string StringValue)
        {
            AddValueLabel(Name, (Int32)(0.GetMissingValue(mv)), StringValue);
        }

        public void AddValueLabel(string Name, Int32 Value, string StringValue)
        {
            if (st == null)
                throw new Exception("File not open for Data Add");

            if (!ValueLabels.ContainsKey(Name))
            {
                ValueLabels.Add(Name, new SortedDictionary<Int32, string>());
            }
            var vl = ValueLabels[Name];
            if (vl.ContainsKey(Value))
            {
                if (vl[Value] != StringValue)
                    throw new Exception("Only on StringValue per Value is allowed");
                return;
            }
            vl.Add(Value, StringValue);
        }
        #endregion

        #region AppendDataLine
        public void AppendDataLine(params object[] data)
        {
            AppendDataLineArray(data);
        }

        public void AppendDataLineArray(object[] data)
        {
            if (st == null)
                throw new Exception("File not open for Data Add");

            if (data == null)
                throw new ArgumentNullException("data");

            if (data.Length != vars.Count)
                throw new ArgumentException("Count of Variables is wrong");

            var writeData = new List<Action>();
            int i = 0;
            foreach (var item in vars)
            {
                var dataItem = data[i];
                if (dataItem == null)
                    dataItem = StataMissingValues._empty;

                StataMissingValues? mv = null;
                if (dataItem is StataMissingValues)
                    mv = (StataMissingValues)data[i];

                switch (item.VarType)
                {
                    #region Byte
                    case StataVariable.StataVarType.Byte:
                        {
                            sbyte b = 0;
                            if (mv.HasValue)
                                b = b.GetMissingValue(mv);
                            else
                                b = Convert.ToSByte(dataItem);
                            writeData.Add(new Action(() => bw.Write(b)));
                        }
                        break;
                    #endregion

                    #region Int
                    case StataVariable.StataVarType.Int:
                        {
                            Int16 b = 0;
                            if (mv.HasValue)
                                b = b.GetMissingValue(mv);
                            else
                                b = Convert.ToInt16(data[i]);
                            writeData.Add(new Action(() => bw.Write(b)));
                        }
                        break;
                    #endregion

                    #region Long
                    case StataVariable.StataVarType.Long:
                        {
                            Int32 b = 0;
                            if (mv.HasValue)
                                b = b.GetMissingValue(mv);
                            else
                                b = Convert.ToInt32(data[i]);
                            writeData.Add(new Action(() => bw.Write(b)));
                        }
                        break;
                    #endregion

                    #region Float
                    case StataVariable.StataVarType.Float:
                        {
                            float f = 0;
                            if (mv.HasValue)
                                f = f.GetMissingValue(mv);
                            else
                                f = Convert.ToSingle(data[i]);
                            writeData.Add(new Action(() => bw.Write(f)));
                        }
                        break;
                    #endregion

                    #region Double
                    case StataVariable.StataVarType.Double:
                        {
                            Double d = 0;
                            if (mv.HasValue)
                                d = d.GetMissingValue(mv);
                            else
                                d = Convert.ToDouble(data[i]);
                            writeData.Add(new Action(() => bw.Write(d)));
                        }
                        break;
                    #endregion

                    #region Fixed Strings
                    case StataVariable.StataVarType.FixedString:
                        {
                            string s = data[i].ToString().PadRight(item.FixedStringLength, (char)0x00).Substring(0, item.FixedStringLength);
                            writeData.Add(new Action(() => bw.Write(env.GetString(outputEncoding.GetBytes(s)))));
                        }
                        break;
                    #endregion

                    #region GSO Strings
                    case StataVariable.StataVarType.String:
                        {
                            string s = env.GetString(outputEncoding.GetBytes(data[i].ToString()));

                            UInt32 v = 0;
                            UInt32 o = 0;

                            if (s != "")
                            {
                                v = (UInt32)(i + 1);
                                o = dataCount + (UInt32)1;
                                if (!smallMemoryFootprint)
                                {
                                    #region local optimized Cache
                                    if (!GSO.ContainsKey(v))
                                        GSO.Add(v, new Dictionary<string, UInt32>());

                                    if (GSO[v].ContainsKey(s))
                                        // already in memory, find object number
                                        o = GSO[v][s];
                                    else
                                        // not in memory add new
                                        GSO[v].Add(s, o);
                                    //var idx = GSO[v].IndexOf(s);
                                    //o = (UInt32)idx;
                                    //idx = -1;
                                    //if (idx == -1)
                                    //{
                                    //    GSO[v].Add(s);
                                    //    o = (UInt32)GSO[v].Count;
                                    //}     
                                    #endregion
                                }
                                else
                                    // direct write to tmp GSO File
                                    tmpGSObw.WriteGSO(v, o, s);

                            }

                            writeData.Add(new Action(() =>
                            {
                                bw.Write(v);
                                bw.Write(o);
                            }));
                        }
                        break;
                        #endregion
                }
                i++;
            }

            foreach (var item in writeData)
                item.Invoke();

            dataCount++;
        }
        #endregion

        #region Close
        public void Close()
        {
            bw.Write("</data>");

            Map.i11_strls = (ulong)st.Position;
            #region Write GSOs
            bw.Write("<strls>");

            if (smallMemoryFootprint)
            {
                byte[] buf = new byte[1048576]; // 1MB Buffer
                tmpGSOst.Seek(0, SeekOrigin.Begin);
                int bytesRead = 0;
                while ((bytesRead = tmpGSOst.Read(buf, 0, buf.Length)) > 0)
                {
                    bw.Write(buf, 0, bytesRead);
                };

                tmpGSOst.Close();
                try
                {
                    File.Delete(tmpGSOfn);
                }
                catch
                {
                    // TODO: logging Framework
                }
            }
            else
            {
                foreach (var GSOVarItem in GSO)
                    foreach (var item in GSOVarItem.Value)
                        bw.WriteGSO((UInt32)GSOVarItem.Key, item.Value, item.Key);
            }

            bw.Write("</strls>");
            #endregion

            Map.i12_value_labels = (ulong)st.Position;

            #region Write Value Labels
            bw.Write("<value_labels>");
            foreach (var VLItem in ValueLabels)
            {
                bw.Write("<lbl>");
                var data = VLItem.Value.ToList();
                bw.Write((UInt32)0); // Total Size of ValueData Later
                bw.Write(GetZeroedFixedString(VLItem.Key, 32));
                bw.Write("   ");
                var fpos = bw.BaseStream.Position;
                bw.Write((UInt32)0); // Number of entries
                bw.Write((UInt32)0); // length of TXT                
                UInt32 txtSize = 0;
                int counter = 0;
                foreach (var item in VLItem.Value)
                {
                    bw.Write((UInt32)txtSize);
                    txtSize += (UInt32)(item.Value.Length + 1);
                    counter++;
                }
                foreach (var item in VLItem.Value)
                {
                    bw.Write((UInt32)item.Key);
                }
                foreach (var item in VLItem.Value)
                {                   
                    //bw.Write(env.GetString(Encoding.Default.GetBytes(item.Value)));
                    bw.Write(env.GetString(outputEncoding.GetBytes(item.Value)));
                    bw.Write((byte)0x00);
                }
                var totalDataSize = bw.BaseStream.Position - fpos;
                bw.BaseStream.Position = fpos;
                bw.Write((UInt32)counter); // Number of entries
                bw.Write((UInt32)txtSize); // length of TXT
                bw.BaseStream.Position = fpos - 40;
                bw.Write((UInt32)totalDataSize); // Total Size of ValueData Later
                bw.BaseStream.Position = totalDataSize + fpos;
                bw.Write("</lbl>");
            }
            bw.Write("</value_labels>");
            #endregion

            Map.i13_Stata_dataEnd = (ulong)st.Position;
            bw.Write("</stata_dta>");
            Map.i14_end_of_file = (ulong)st.Position;

            // Write Count of Data
            st.Seek((long)(Map.i01_stata_data + 79), SeekOrigin.Begin);
            bw.Write(dataCount);

            #region Write MAP Block
            st.Seek((long)(Map.i02_map + 5), SeekOrigin.Begin);
            foreach (var item in Map.GetData())
                bw.Write(item);
            #endregion

            st.Close();
            st = null;
        }
        #endregion
    }
    #endregion

    #region StataBinaryReader
    internal class StataBinaryReader : BinaryReader
    {
        private static Encoding stataEncoding = Encoding.GetEncoding(1252);

        public StataBinaryReader(Stream output)
            : base(output, Encoding.GetEncoding(1252))
        { }

        public string ReadStringByteLen()
        {
            var len = ReadByte();
            byte[] data = new byte[len];
            Read(data, 0, (int)len);
            return stataEncoding.GetString(data);
        }

        public void ReadExcpectedString(string expected)
        {
            var readSt = ReadString(expected.Length);
            if (readSt != expected)
                throw new Exception("Expected " + expected + " found " + readSt);
        }

        public string ReadString(int len)
        {
            byte[] data = new byte[len];
            Read(data, 0, len);
            return stataEncoding.GetString(data);
        }

        public string ReadString(UInt32 len)
        {
            byte[] data = new byte[len];
            Read(data, 0, (int)len);
            return stataEncoding.GetString(data);
        }

        public string ReadStringZeroed()
        {
            string s = "";
            char b;
            while ((b = ReadChar()) != (char)0x00)
            {
                s += b;
            }
            return s;
        }

        public string ReadStringZeroed(int len)
        {
            var s = ReadString(len + 1);
            var idx = s.IndexOf((char)0x00);
            if (idx != -1)
                s = s.Substring(0, idx);
            return s;
        }

        public Tuple<UInt32, UInt32, string> ReadGSO()
        {
            var gso = ReadString(3); //"GSO");
            var v = ReadUInt32();
            var o = ReadUInt32();
            var t = ReadByte(); //     Write((byte)(130)); // TODO: TYPE ASCII -> Binary 129 Check if \0x00 contains 
            var len = ReadUInt32();

            var str = ReadString(len);
            var idx = str.IndexOf((char)0x00);
            if (idx != -1)
                str = str.Substring(0, idx);
            // TODO check last char for 0x00
            //Write((byte)0x00);

            return new Tuple<uint, uint, string>(v, o, str);
        }

        public Tuple<UInt32, UInt32, Int64, Int64> ReadGSOCache()
        {
            var gso = ReadString(3); //"GSO");
            var v = ReadUInt32();
            var o = ReadUInt32();
            var t = ReadByte(); //     Write((byte)(130)); // TODO: TYPE ASCII -> Binary 129 Check if \0x00 contains 
            var len = ReadUInt32();
            var pos = this.BaseStream.Position;
            this.BaseStream.Seek(len, SeekOrigin.Current);
            return new Tuple<uint, uint, Int64, Int64>(v, o, pos, (Int64)len);
        }
    }
    #endregion

    #region StataFileReader
    public class StataFileReader : IEnumerable<object[]>
    {
        #region Variables & Properties
        private string filename;
        private bool smallMemoryFootprint = false;

        private StataBinaryReader br = null;

        private Stream st = null;

        private UInt32 dataCount = 0;
        public UInt32 Count
        {
            get
            {
                return dataCount;
            }
        }

        private string dataName = "";
        public string DataName
        {
            get
            {
                return dataName;
            }
        }

        private DateTime creationDate;
        public DateTime CreationData
        {
            get
            {
                return creationDate;
            }
        }

        private StataMap Map;

        private List<StataVariable> variables;
        public List<StataVariable> Variables
        {
            get
            {
                return (from c in variables select c.Clone()).ToList();
            }
        }

        private int DataLineLength = 0;

        private Dictionary<string, Dictionary<Int32, string>> valueLabels = new Dictionary<string, Dictionary<Int32, string>>();

        public List<Tuple<string, Int32, string>> ValueLabels
        {
            get
            {
                var result = new List<Tuple<string, Int32, string>>();
                foreach (var vlList in valueLabels)
                {
                    foreach (var item in vlList.Value)
                    {
                        result.Add(new Tuple<string, Int32, string>(vlList.Key, item.Key, item.Value));
                    }
                }
                return result;
            }
        }

        private Dictionary<UInt32, Dictionary<UInt32, string>> GSO = new Dictionary<UInt32, Dictionary<UInt32, string>>();
        private Dictionary<UInt32, Dictionary<UInt32, Tuple<Int64, Int64>>> GSOcache = new Dictionary<UInt32, Dictionary<UInt32, Tuple<Int64, Int64>>>();
        #endregion

        #region Constructor

        #region Mapping Constructors
        public StataFileReader(string FileName)
            : this(FileName, false)
        {
        }

        public StataFileReader(string FileName, bool SmallMemoryFootprint)
            : this(File.Open(FileName, FileMode.Open), SmallMemoryFootprint)
        {
            this.filename = FileName;
        }

        public StataFileReader(Stream st)
            : this(st, false)
        {
        }
        #endregion

        public StataFileReader(Stream st, bool SmallMemoryFootprint)
        {
            this.smallMemoryFootprint = SmallMemoryFootprint;
            this.st = st;
            if (!st.CanSeek)
                throw new ArgumentException("Base Stream has to support Seek");

            br = new StataBinaryReader(st);
            br.ReadExcpectedString("<stata_dta>");

            #region Header
            br.ReadExcpectedString("<header>");
            br.ReadExcpectedString("<release>117</release>");
            br.ReadExcpectedString("<byteorder>" + (BitConverter.IsLittleEndian ? "LSF" : "MSF") + "</byteorder>");
            br.ReadExcpectedString("<K>");
            var varCount = br.ReadUInt16();
            br.ReadExcpectedString("</K>");

            br.ReadExcpectedString("<N>");
            dataCount = br.ReadUInt32();
            br.ReadExcpectedString("</N>");


            br.ReadExcpectedString("<label>");
            dataName = br.ReadStringByteLen();
            br.ReadExcpectedString("</label>");

            br.ReadExcpectedString("<timestamp>" + (char)17);
            var tt = br.ReadString(17);
            if (tt[0] == ' ') tt = "0" + tt.Substring(1);
            creationDate = DateTime.ParseExact(tt, "dd MMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            br.ReadExcpectedString("</timestamp>");
            br.ReadExcpectedString("</header>");
            #endregion

            #region Map
            Map = new StataMap();
            br.ReadExcpectedString("<map>");
            Map.i01_stata_data = br.ReadUInt64();
            Map.i02_map = br.ReadUInt64();
            Map.i03_vartypes = br.ReadUInt64();
            Map.i04_varnames = br.ReadUInt64();
            Map.i05_sortlist = br.ReadUInt64();
            Map.i06_formats = br.ReadUInt64();
            Map.i07_value_label_names = br.ReadUInt64();
            Map.i08_variable_labels = br.ReadUInt64();
            Map.i09_characteristics = br.ReadUInt64();
            Map.i10_data = br.ReadUInt64();
            Map.i11_strls = br.ReadUInt64();
            Map.i12_value_labels = br.ReadUInt64();
            Map.i13_Stata_dataEnd = br.ReadUInt64();
            Map.i14_end_of_file = br.ReadUInt64();
            br.ReadExcpectedString("</map>");
            #endregion

            #region Variables
            variables = new List<StataVariable>();
            for (int i = 0; i < varCount; i++)
                variables.Add(new StataVariable());

            st.Position = (long)Map.i03_vartypes;
            br.ReadExcpectedString("<variable_types>");
            foreach (var item in variables)
            {
                var tmpType = br.ReadUInt16();

                if (tmpType <= 2045)
                {
                    item.FixedStringLength = tmpType;
                    item.VarType = StataVariable.StataVarType.FixedString;
                    DataLineLength += tmpType;
                }
                else
                {
                    item.VarType = (StataVariable.StataVarType)((int)tmpType);
                    switch (item.VarType)
                    {
                        case StataVariable.StataVarType.Byte:
                            DataLineLength += 1;
                            break;
                        case StataVariable.StataVarType.Int:
                            DataLineLength += 2;
                            break;
                        case StataVariable.StataVarType.Float:
                        case StataVariable.StataVarType.Long:
                            DataLineLength += 4;
                            break;
                        case StataVariable.StataVarType.String:
                        case StataVariable.StataVarType.Double:
                            DataLineLength += 8;
                            break;
                        default:
                            throw new Exception("Unknown Datatype: " + tmpType.ToString());
                    }
                }
            }
            br.ReadExcpectedString("</variable_types>");

            st.Position = (long)Map.i04_varnames;
            br.ReadExcpectedString("<varnames>");
            foreach (var item in variables)
                item.Name = br.ReadStringZeroed(32);
            br.ReadExcpectedString("</varnames>");


            if (Map.i05_sortlist > 0)
            {
                st.Position = (long)Map.i05_sortlist;
                br.ReadExcpectedString("<sortlist>");
                // TODO: add Sortlist Feature
                //foreach (var item in vars)
                //{
                //    
                //    br.ReadUInt16(); 
                //}
                //br.ReadUInt16();
                //bw.Write("</sortlist>");
            }

            if (Map.i06_formats > 0)
            {
                st.Position = (long)Map.i06_formats;
                br.ReadExcpectedString("<formats>");
                foreach (var item in variables)
                    item.DisplayFormat = br.ReadStringZeroed(48);
            }

            br.ReadExcpectedString("</formats>");

            if (Map.i07_value_label_names > 0)
            {
                st.Position = (long)Map.i07_value_label_names;
                br.ReadExcpectedString("<value_label_names>");
                foreach (var item in variables)
                    item.ValueLabelName = br.ReadStringZeroed(32);
                br.ReadExcpectedString("</value_label_names>");
            }

            if (Map.i08_variable_labels > 0)
            {
                st.Position = (long)Map.i08_variable_labels;
                br.ReadExcpectedString("<variable_labels>");
                foreach (var item in variables)
                    item.Description = br.ReadStringZeroed(80);

                br.ReadExcpectedString("</variable_labels>");
            }

            if (Map.i09_characteristics > 0)
            {
                st.Position = (long)Map.i09_characteristics;
                br.ReadExcpectedString("<characteristics>");
                // don't read characteristics
                //</characteristics>"); 
            }
            #endregion

            st.Position = (long)Map.i10_data;
            br.ReadExcpectedString("<data>");

            #region GSOs
            if (Map.i11_strls > 0)
            {
                st.Position = (long)Map.i11_strls;
                br.ReadExcpectedString("<strls>");
                while (br.ReadString(3) == "GSO")
                {
                    st.Seek(-3, SeekOrigin.Current);
                    if (smallMemoryFootprint)
                    {
                        var GSOCacheItem = br.ReadGSOCache();

                        if (!GSOcache.ContainsKey(GSOCacheItem.Item1))
                            GSOcache.Add(GSOCacheItem.Item1, new Dictionary<uint, Tuple<long, long>>());

                        GSOcache[GSOCacheItem.Item1].Add(GSOCacheItem.Item2, new Tuple<long, long>(GSOCacheItem.Item3, GSOCacheItem.Item4));
                    }
                    else
                    {
                        var GSOItem = br.ReadGSO();

                        if (!GSO.ContainsKey(GSOItem.Item1))
                            GSO.Add(GSOItem.Item1, new Dictionary<uint, string>());

                        GSO[GSOItem.Item1].Add(GSOItem.Item2, GSOItem.Item3);
                    }
                }
                st.Seek(-3, SeekOrigin.Current);

                br.ReadExcpectedString("</strls>");
            }
            #endregion

            #region ValueLabels
            if (Map.i12_value_labels > 0)
            {
                st.Position = (long)Map.i12_value_labels;
                br.ReadExcpectedString("<value_labels>");
                while (br.ReadString(5) == "<lbl>")
                {
                    var totSize = br.ReadUInt32();
                    var vlName = br.ReadStringZeroed(32);
                    br.ReadString(3);
                    var numEntries = br.ReadUInt32();
                    var lenTXT = br.ReadUInt32();

                    var tmpVLentries = new Int32[numEntries, 2];

                    for (int i = 0; i < numEntries; i++)
                        tmpVLentries[i, 0] = br.ReadInt32(); // TXT Offset for Element

                    for (int i = 0; i < numEntries; i++)
                        tmpVLentries[i, 1] = br.ReadInt32(); // Value for Element              

                    var vlDic = new Dictionary<Int32, string>();
                    var ll = st.Position;
                    for (int i = 0; i < numEntries; i++)
                    {
                        st.Position = ll + tmpVLentries[i, 0];

                        vlDic.Add(tmpVLentries[i, 1], br.ReadStringZeroed());
                    }
                    st.Position = ll + lenTXT;
                    br.ReadExcpectedString("</lbl>");

                    valueLabels.Add(vlName, vlDic);
                }
                br.ReadExcpectedString("ue_labels>"); //  the first 5 charcaters are already read "</val" 
            }
            #endregion

            st.Position = (long)Map.i13_Stata_dataEnd;
            br.ReadExcpectedString("</stata_dta>");

            if (Map.i14_end_of_file != (ulong)st.Position)
                throw new Exception("Stat File has a wrong length. Expected: " + Map.i14_end_of_file.ToString() + " found: " + st.Position.ToString());
        }
        #endregion

        #region ReadDataLine
        public object[] ReadDataLine(int index)
        {
            if (st == null)
                throw new Exception("File already closed");

            if (index < 0 || index >= dataCount)
            {
                throw new ArgumentException("Index is out of Range");
            }

            st.Position = (long)(Map.i10_data) + 6 + index * DataLineLength;
            var result = new object[variables.Count];
            for (int i = 0; i < variables.Count; i++)
            {
                switch (variables[i].VarType)
                {
                    case StataVariable.StataVarType.Byte:
                        result[i] = br.ReadSByte();
                        break;
                    case StataVariable.StataVarType.Int:
                        result[i] = br.ReadInt16();
                        break;
                    case StataVariable.StataVarType.Long:
                        result[i] = br.ReadInt32();
                        break;
                    case StataVariable.StataVarType.Float:
                        result[i] = br.ReadSingle();
                        break;
                    case StataVariable.StataVarType.Double:
                        result[i] = br.ReadDouble();
                        break;
                    case StataVariable.StataVarType.FixedString:
                        result[i] = br.ReadStringZeroed(variables[i].FixedStringLength - 1);
                        break;
                    case StataVariable.StataVarType.String:
                        var v = br.ReadUInt32();
                        var o = br.ReadUInt32();
                        if (v == 0 && o == 0)
                        {
                            result[i] = "";
                        }
                        else if (smallMemoryFootprint)
                        {
                            var tmpPos = st.Position;
                            var tp = GSOcache[v][o];
                            st.Position = tp.Item1;
                            var s = br.ReadString((int)tp.Item2);
                            var idx = s.IndexOf((char)0x00);
                            if (idx != -1)
                                s = s.Substring(0, idx);
                            result[i] = s;
                            st.Position = tmpPos;
                        }
                        else
                            result[i] = GSO[v][o];
                        break;
                }
            }

            return result;
        }
        #endregion

        #region Close
        public void Close()
        {
            st.Close();
            st = null;
        }
        #endregion

        #region Enumerator
        public IEnumerator<object[]> GetEnumerator()
        {
            for (int i = 0; i < dataCount; i++)
                yield return ReadDataLine(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    #endregion
}