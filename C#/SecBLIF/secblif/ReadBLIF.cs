﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SecBLIF
{
    enum VerilogOutputType
    {
        COMBLOG_COMP,
        COMBLOG_MIN,
        LUTPRIM
    }

    enum LUTPriotitizationType
    {
        FANOUT,
        O_C
    }

    enum Manufacturer
    {
        ALTERA,
        XILINX
    }

    class BLIFFILE
    {
        public string CKT_NAME { get; set; }
        public bool embed_watermark { get; set; }
        public string HMAC { get; set; }
        public  List<LUT> CKT_LUTS;

        public List<List<LUT>> PartitionedLUTs { get; set; }
        public List<string> InstantiationTemplates { get; set; }
        public int[] SecretKeys { get; set; }

        public List<string> CKT_INPUTS { get; set; }
        public List<string> CKT_OUTPUTS { get; set; }
        public List<string> CKT_WIRES { get; set; }
        public List<Latch> CKT_LATCHES { get; set; }
        public List<string> CKT_REGS { get; set; }
        public Dictionary<string, int> keyDistribution { get; set; }
        public decimal obfuscationPercentage { get; set; }
        public int LUTsPerKey { get; set; }
        public int keyBits { get; set; }
        public double avgSimilarity { get; set; }
        public bool LUTsShareKey { get; set; }
        public bool IsSecure { get; set; }
        public int[] LUTSizeCount { get; set; }
        public int MaxLUTSize { get; set; }
        public Manufacturer manufacturer;
        string sprng_key { get; set; }
        List<string> sprng_keys { get; set; }
        int maxLUTID { get; set; }

        public BLIFFILE(int max_lut_size)
        {
            CKT_LUTS = new List<LUT>();
            PartitionedLUTs = new List<List<LUT>>();
            SecretKeys = new int[100];
            CKT_INPUTS = new List<string>();
            CKT_OUTPUTS = new List<string>();
            CKT_WIRES = new List<string>();
            CKT_LATCHES = new List<Latch>();
            CKT_REGS = new List<string>();
            InstantiationTemplates = new List<string>();

            // manufacturer = man;
            // share up to 10 for now, maybe use some function based on circuit size and obfus percentage later
            // LUTsPerKey = 10; 
            sprng_key = string.Empty;
            sprng_keys = new List<string>();
            //LUTsShareKey = false;
            IsSecure = false;
            MaxLUTSize = max_lut_size;
            LUTSizeCount = new int[max_lut_size + 15]; // supporting at most 8 input LUTs for now (8/22/18 - adjusted for testing purposes)
        }

        internal void PrintStats()
        {
            if(IsSecure)
                Console.WriteLine("\n\tBLIF Statistics {0}% Obfuscation:", obfuscationPercentage*100);
            else
                Console.WriteLine("\n\tBLIF Statistics:");
            Console.WriteLine("\tNumber of inputs:".PadRight(25) + "\t" + CKT_INPUTS.Count);
            Console.WriteLine("\tNumber of outputs:".PadRight(25) + "\t" + CKT_OUTPUTS.Count);
            Console.WriteLine("\tNumber of LUTs:".PadRight(25) + "\t" + CKT_LUTS.Count);

            for(int i = 1; i < LUTSizeCount.Length; i++)
                Console.WriteLine("\t\t{0} Inputs:".PadRight(20) + "\t" + LUTSizeCount[i], i);

            int max_size = LUTSizeCount.Length;
            double max_capacity = CKT_LUTS.Count * (int)Math.Pow(2, max_size);
            double current_capacity = 0;
            for (int i = 1; i < max_size; i++)
            {
                current_capacity += (LUTSizeCount[i] * (int)Math.Pow(2, i));
            }
            double occupancy = current_capacity / max_capacity;
            Console.WriteLine("\tOccupancy: ".PadRight(25) + "\t" + "{0:#0.00%}", occupancy);
            Console.WriteLine();
            if (IsSecure)
                Console.WriteLine("\t Key Generated: {0}", sprng_key);
        }

        public void AddLUT(LUT xLUT)
        {
            //xLUT.ConvertToMinterms();
            CKT_LUTS.Add(xLUT);
            LUTSizeCount[xLUT.NumInputs]++;
        }

        internal string WriteBitstream(string filename)
        {
            Util.WriteInfo("Writing bitstream...", true);
            DateTime start = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            int name_start = filename.LastIndexOf("/") + 1;
            if (name_start < 0) name_start = 0;
            string mod_name = filename.Substring(name_start).Replace(".v", "").Replace(".dat", "");

            //filename = filename.Insert(filename.LastIndexOf(".v"), "_bitstream");
            //filename = filename.Replace(".v", ".dat");

            if (IsSecure)
            {
                if (LUTsShareKey)
                {
                    filename = filename.Insert(filename.LastIndexOf("/") + 1, "ks_");
                }
                else
                {
                    filename = filename.Insert(filename.LastIndexOf("/") + 1, "ns_");
                }
            }

            TextWriter w = new StreamWriter(filename);

            if(IsSecure && embed_watermark)
            {
                StringBuilder sb2 = new StringBuilder();
                TextWriter w2 = new StreamWriter(filename.Replace(".dat", "") + "_lutstruct.dat");

                foreach(LUT xLUT in CKT_LUTS)
                {
                    sb2.Append(xLUT.NumInputs + ",");
                }
                sb2.Remove(sb2.Length - 1, 1);
                w2.Write(sb2.ToString());
                w2.Close();
            }

            foreach (LUT xLUT in CKT_LUTS)
            {
                sb.Append(xLUT.expandTruthTable());
            }
            w.Write(sb.ToString());

            w.Close();

            DateTime end = DateTime.Now;

            var diff = end - start;
            Util.WriteInfo(String.Format("Done. ({0:0.000} s)\n", diff.TotalSeconds), false);
            return sb.ToString();
        }

        internal Dictionary<string, int> parseVectors(List<string> nodes)
        {
            // assuming node names in form <text>~<index>~
            List<string> nodeGroups = new List<string>();
            Dictionary<string, int> vectors = new Dictionary<string, int>();

            for(int i = 0; i < nodes.Count; i++)
            {
                string[] vec = nodes[i].Split('~');
                if (vec.Length == 1)
                {
                    nodeGroups.Add(vec[0]);
                    vectors.Add(vec[0], 0);
                    continue;
                }
                string vName = vec[0];
                int vIdx = Convert.ToInt32(vec[1]);
                if (!nodeGroups.Contains(vec[0]))
                {
                    nodeGroups.Add(vName);
                    vectors.Add(vName, vIdx);
                }
                else
                {
                    if (vIdx > vectors[vName])
                    {
                        vectors[vName] = vIdx;
                    }
                }
            }

            return vectors;
        }

        internal void WriteVerilog(string filename, VerilogOutputType vot)
        {
            if (vot == VerilogOutputType.COMBLOG_MIN)
               Util.WriteInfo("Writing minimized verilog...", true);
            else
                Util.WriteInfo("Writing verilog...", true);
            DateTime start = DateTime.Now;
            
            int name_start = filename.LastIndexOf("/") + 1;
            if (name_start < 0) name_start = 0;
            string mod_name = filename.Substring(name_start).Replace(".v", "").Replace('.', 'x');

            //mod_name = mod_name + "_map";
            ////filename = filename.Insert(filename.LastIndexOf(".v"), "_map");

            //if (IsSecure)
            //{
            //    if (LUTsShareKey)
            //    {
            //        filename = filename.Insert(filename.LastIndexOf("/") + 1, "ks_");
            //        mod_name = "ks_" + mod_name;
            //    }
            //    else
            //    {
            //        filename = filename.Insert(filename.LastIndexOf("/") + 1, "ns_");
            //        mod_name = "ns_" + mod_name;
            //    }
            //}

            TextWriter w = new StreamWriter(filename);

            if (this.IsSecure)
                w.WriteLine("// sk = {0}", this.sprng_key);

            w.WriteLine("`timescale 10ns/1ns");

            if (IsSecure)
                w.Write("module {0} (/* input clk, */ input [{1}:0] sk, ", mod_name, /*LUTsPerKey*/ keyBits - 1);
            else
                w.Write("module {0} (/* input clk, */ ", mod_name);
            Dictionary<string, int> input_vectors = parseVectors(CKT_INPUTS);
            Dictionary<string, int> output_vectors = parseVectors(CKT_OUTPUTS);
            Dictionary<string, int> wire_vectors = parseVectors(CKT_WIRES);
            Dictionary<string, int> reg_vectors = parseVectors(CKT_REGS);

            int nIVs = 0;
            foreach(KeyValuePair<string, int> vector in input_vectors)
            {
                if (nIVs == 0)
                {
                    nIVs++;
                    if (vector.Value > 0)
                        w.Write("input [{0}:0] {1}", vector.Value, vector.Key);
                    else
                        w.Write("input {0}",vector.Key);
                }
                else
                {
                    if (vector.Value > 0)
                        w.Write(", input [{0}:0] {1}", vector.Value, vector.Key);
                    else
                        w.Write(", input {0}", vector.Key);
                }
            }
            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                //if (vector.Key.StartsWith("reg "))
                //{
                if (reg_vectors.ContainsKey(vector.Key))
                {
                    if (vector.Value > 0)
                        w.Write(", output reg [{0}:0] {1}", vector.Value, vector.Key);
                    else
                        w.Write(", output reg {0}", vector.Key);
                }
                else
                {
                    if (vector.Value > 0)
                        w.Write(", output wire [{0}:0] {1}", vector.Value, vector.Key);
                    else
                        w.Write(", output wire {0}", vector.Key);
                }
                //}
                //else
                //{
                //    if (vector.Value > 0)
                //        w.Write(", output [{0}:0] {1}", vector.Value, vector.Key);
                //    else
                //        w.Write(", output {0}", vector.Key);
                //}
            }

            //for (int i = 0; i < CKT_INPUTS.Count; i++)
            //{

            //    w.Write(CKT_INPUTS[i] + ", ");
            //}
            //for(int i = 0; i < CKT_OUTPUTS.Count; i++)
            //{
            //    if (CKT_OUTPUTS[i].StartsWith("reg "))
            //        w.Write(CKT_OUTPUTS[i].Substring(4));
            //    else
            //        w.Write(CKT_OUTPUTS[i]);
            //    if (i != CKT_OUTPUTS.Count - 1)
            //        w.Write(", ");
            //}
            w.Write(");\n");
            w.Write("\n");
            //for (int i = 0; i < CKT_INPUTS.Count; i++)
            //{
            //    w.WriteLine("\tinput {0};", CKT_INPUTS[i]);
            //}
            //for (int i = 0; i < CKT_OUTPUTS.Count; i++)
            //{
            //    w.WriteLine("\toutput {0};", CKT_OUTPUTS[i]);
            //}
            w.WriteLine();
            //if (IsSecure)
            //{
            //    if (LUTsShareKey)
            //    {
            //        int keybits = LUTsPerKey;// (int)Math.Ceiling((double)CKT_LUTs.Count / (double)LUTsPerKey);
            //        w.Write("\tinput [{0} : 0] sk /* synthesis noprune */;\n", keybits - 1);
            //    }
            //    else
            //        w.Write("\tinput [{0} : 0] sk /* synthesis noprune */;\n", CKT_LUTS.Count - 1);
            //}
            //w.WriteLine();
            int cnt = 0, elemsPerLine = 10;

            if(vot == VerilogOutputType.COMBLOG_COMP)
            {
                StringBuilder sb = new StringBuilder();
                string initial_vals = string.Empty;
                foreach(Latch latch in CKT_LATCHES)
                {
                    initial_vals += latch.init_val;
                }

                if(initial_vals != string.Empty)
                {
                    sb.AppendFormat("initial begin\n {0}\nend\n", initial_vals);
                    w.Write(sb.ToString());
                }
            }

            w.WriteLine("//reg [{0}:0] inputs;", CKT_INPUTS.Count - 1);
            w.WriteLine("//reg [{0}:0] outputs;", CKT_OUTPUTS.Count - 1);

            //if (this.IsSecure)
            //{
            //    w.WriteLine("wire satwreck;");

            //    w.Write("sat_module sat_resilience (.inputs({");
            //    for (int i = 0; i < this.CKT_INPUTS.Count - 1; i++)
            //        w.Write("{0}, ", CKT_INPUTS[i]);

            //    w.Write("{0} }}), .keybits(sk[{1}:0]), .sat_out(satwreck));\n", CKT_INPUTS[CKT_INPUTS.Count - 1], CKT_INPUTS.Count - 1);
            //}


            if (this.IsSecure)
                w.WriteLine("//reg [{0}:0] sk_reg;", keyBits - 1);

            w.Write("//always @(posedge clk) inputs = {");
            for (int i = 0; i < CKT_INPUTS.Count - 1; i++)
                w.Write("{0}, ", CKT_INPUTS[i]);

            w.Write(CKT_INPUTS[CKT_INPUTS.Count - 1]);
            w.Write("};\n");

            w.Write("//always @(posedge clk) outputs = {");
            for (int i = 0; i < CKT_OUTPUTS.Count - 1; i++)
                w.Write("{0}, ", CKT_OUTPUTS[i]);

            w.Write(CKT_OUTPUTS[CKT_OUTPUTS.Count - 1]);
            w.Write("};\n");

            if (this.IsSecure)
                w.WriteLine("//always @(posedge clk) sk_reg = sk;");

            if (this.IsSecure)
                w.WriteLine("// localparam sk = {0}'b{1};", keyBits, this.sprng_key);

            //foreach(string s in CKT_WIRES)
            foreach (KeyValuePair<string, int> vector in wire_vectors)
            {
                if (CKT_INPUTS.Contains(vector.Key) || CKT_OUTPUTS.Contains(vector.Key) || 
                    input_vectors.ContainsKey(vector.Key) || output_vectors.ContainsKey(vector.Key) || vector.Value == 0)
                    continue;
                w.WriteLine("\n\twire [{0}:0] {1};", vector.Value, vector.Key);
            }
            foreach(KeyValuePair<string, int> vector in wire_vectors)
            {
                if (input_vectors.ContainsKey(vector.Key) || output_vectors.ContainsKey(vector.Key) || vector.Value > 0)
                    continue;
                if (cnt == 0)
                {
                    w.Write("\n\twire {0}", vector.Key);
                    cnt++;
                }
                else if (cnt < elemsPerLine)
                {
                    w.Write(", {0}", vector.Key);
                    cnt++;
                }
                else
                {
                    w.Write(", {0};", vector.Key);
                    cnt = 0;
                }
            }
            if (cnt != 0)
                w.Write(";\n");

            w.WriteLine();
            cnt = 0;
            elemsPerLine = 8;
            foreach (KeyValuePair<string, int> vector in reg_vectors)
            {
                if (vector.Value == 0 || output_vectors.ContainsKey(vector.Key))
                    continue;
                w.WriteLine("\n\treg [{0}:0] {1};", vector.Value, vector.Key);
            }
            //foreach (string s in CKT_REGS)'
            foreach (KeyValuePair<string, int> vector in reg_vectors)
            {
                if (vector.Value > 0 || CKT_OUTPUTS.Contains(vector.Key))
                    continue;
                if (cnt == 0)
                {
                    w.Write("\n\treg {0}", vector.Key);
                    cnt++;
                }
                else if (cnt < elemsPerLine)
                {
                    w.Write(", {0}", vector.Key);
                    cnt++;
                }
                else
                {
                    w.Write(", {0};", vector.Key);
                    cnt = 0;
                }
            }
            if (cnt != 0)
                w.Write(";\n");

            w.WriteLine();

            //if (IsSecure)
            //{
            //    // XOR all primary outputs with SARLock flip/mask unit
            //    foreach (LUT lut in CKT_LUTS)
            //    {
            //        if (!CKT_OUTPUTS.Contains(lut.LUToutput.Replace('[', '~').Replace(']', '~'))) continue;
            //        //if (lut.NumInputs >= MAXLUTSIZE) continue;

            //        lut.LUTinputs.Insert(0, "satwreck");

            //        Dictionary<string, string> minterms = new Dictionary<string, string>();
            //        foreach (KeyValuePair<string, string> kvp in lut.TruthTable)
            //        {
            //            string minterm = "0" + kvp.Key;
            //            minterms.Add(minterm, kvp.Value);
            //        }

            //        lut.TruthTable = minterms;
            //        lut.NumInputs++;
            //        lut.ContentSize *= 2;
            //    }
            //}

            if (vot == VerilogOutputType.COMBLOG_COMP)
            {
                foreach (Latch latch in CKT_LATCHES)
                {
                    w.WriteLine(latch.ToString());
                }
            }

            w.WriteLine();
            
            if (vot == VerilogOutputType.LUTPRIM)
            {
                foreach (Latch latch in CKT_LATCHES)
                    w.WriteLine(latch.ToString());

                foreach (LUT xLUT in CKT_LUTS)
                {
                    //if (!xLUT.IsMinimized)
                    //    xLUT.Minimize();
                    w.WriteLine(xLUT.ToPrimitive(manufacturer));
                }
            }
            else if (vot == VerilogOutputType.COMBLOG_COMP)
            {
                foreach (LUT xLUT in CKT_LUTS)
                {
                    w.WriteLine(xLUT.getSOP());
                }
            }
            else if (vot == VerilogOutputType.COMBLOG_MIN)
            {
                foreach (LUT xlut in CKT_LUTS)
                {
                    if(!xlut.IsMinimized)
                        xlut.Minimize();

                    w.WriteLine(xlut.getSOP());
                }
            }
            w.WriteLine();
            w.Write("endmodule");
            w.WriteLine();

            if (vot == VerilogOutputType.LUTPRIM && manufacturer == Manufacturer.ALTERA)
            {
                w.Write("\n\n");
                w.WriteLine("module lut_sub (din,out);");
                w.WriteLine("\tparameter LUT_SIZE = 4;");
                w.WriteLine("\tparameter NUM_BITS = 2**LUT_SIZE;");
                w.WriteLine("\n\tinput[LUT_SIZE - 1:0] din;");
                w.WriteLine("\tparameter[NUM_BITS - 1:0] mask = {NUM_BITS{1'b0}};");
                w.WriteLine("\n\toutput out;");
                w.WriteLine("\n\twire out;");
                w.WriteLine();
                w.WriteLine("\t// buffer the LUT inputs...");
                w.WriteLine("\twire[LUT_SIZE - 1:0] din_w;");
                w.WriteLine();
                w.WriteLine("\tgenvar i;");
                w.WriteLine("\tgenerate");
                w.WriteLine("\t\tfor (i = 0; i < LUT_SIZE; i = i + 1)");
                w.WriteLine("\t\t\tbegin: liloop");
                w.WriteLine("\t\t\tlut_input li_buf(din[i], din_w[i]);");
                w.WriteLine("\t\tend");
                w.WriteLine("\tendgenerate");
                w.WriteLine();
                w.WriteLine("\t// build up the pterms for the LUT");
                w.WriteLine("\twire[NUM_BITS - 1:0] pterms;");
                w.WriteLine("\tgenerate");
                w.WriteLine("\t\tfor (i = 0; i < NUM_BITS; i = i + 1)");
                w.WriteLine("\t\t\tbegin: ploop");
                w.WriteLine("\t\t\tassign pterms[i] = ((din_w == i) & mask[i]);");
                w.WriteLine("\t\tend");
                w.WriteLine("\tendgenerate");
                w.WriteLine();
                w.WriteLine("\t// assign the pterms to the LUT function");
                w.WriteLine("\twire result;");
                w.WriteLine("\tassign result = | pterms;");
                w.WriteLine("\tlut_output lo_buf (result, out);");
                w.WriteLine("endmodule");
            }

            //if (IsSecure)
            //{
            //    w.Write("\n\n");
            //    w.WriteLine("module sat_module (inputs, keybits, sat_out);");
            //    w.WriteLine("\tparameter input_size = {0};", CKT_INPUTS.Count);
            //    var tmp1 = sprng_key.Substring(sprng_key.Length - CKT_INPUTS.Count, CKT_INPUTS.Count).ToArray();
            //    Array.Reverse(tmp1);
            //    var tmp2 = new string(tmp1);

            //    w.WriteLine("\tparameter correct_keybits = {0}'b{1};", CKT_INPUTS.Count, sprng_key.Substring(sprng_key.Length - CKT_INPUTS.Count, CKT_INPUTS.Count));
            //    w.Write("\n");
            //    w.WriteLine("\tinput wire [input_size - 1:0]inputs;");
            //    w.WriteLine("\tinput wire [input_size - 1:0]keybits;");
            //    w.WriteLine("\toutput wire sat_out;");
            //    w.WriteLine("\tassign sat_out = (inputs == keybits) & (keybits != correct_keybits);");
            //    w.WriteLine("endmodule");
            //}

            w.Close();
            DateTime end = DateTime.Now;

            var diff = end - start;
            Console.WriteLine("Done. ({0:0.000} s)", diff.TotalSeconds);
        }

        /*
        internal void MakeSecure(int LUTsPerKey)
        {
            DateTime start = DateTime.Now;
            Util.WriteInfo("Securing BLIF file...", true);
            this.LUTsPerKey = LUTsPerKey;
            LUTsShareKey = true;
            IsSecure = true;
            int numMaxSizeLUT = LUTSizeCount[MaxLUTSize];

            int maxCutoff = (int)(CKT_LUTS.Count * obfuscationPercentage);

            for(int i = 0; i < LUTSizeCount.Length; i++)
            {
                LUTSizeCount[i] = 0;
            }
            Random r = new Random();
            for (int i = 0; i < maxCutoff; i++)
            {
                if (CKT_LUTS[i].NumInputs == MaxLUTSize)
                {
                    numMaxSizeLUT++;
                    continue;
                }
                CKT_LUTS[i].AddKeyInput("1", r.Next(0, CKT_LUTS[i].NumInputs), r.Next(0, LUTsPerKey)); //127));// i % LUTsPerKey); // how does random assignment affect overhead?
                LUTSizeCount[CKT_LUTS[i].NumInputs]++;
            }

            // refill LUTSizeCount that was not obfuscated
            for (int i = maxCutoff; i < CKT_LUTS.Count ; i++)
            {
                LUTSizeCount[CKT_LUTS[i].NumInputs]++;
            }

            LUTSizeCount[MaxLUTSize] += numMaxSizeLUT; // add the original number of MaxLUTSize LUTs back in.
            DateTime end = DateTime.Now;

            var diff = end - start;
            Console.WriteLine("Done. ({0:0.000} s)", diff.TotalSeconds);
        }

        */
        public static void NumericalSort(string[] ar)
        {
            Regex rgx = new Regex("([^0-9]*)([0-9]+)");
            Array.Sort(ar, (a, b) =>
            {
                var ma = rgx.Matches(a);
                var mb = rgx.Matches(b);
                for (int i = 0; i < ma.Count; ++i)
                {
                    int ret = ma[i].Groups[1].Value.CompareTo(mb[i].Groups[1].Value);
                    if (ret != 0)
                        return ret;

                    ret = int.Parse(ma[i].Groups[2].Value) - int.Parse(mb[i].Groups[2].Value);
                    if (ret != 0)
                        return ret;
                }

                return 0;
            });
        }

        internal string GenerateHMAC()
        {
            string hmac = "";

            Random rnd = new Random();

            for (int i = 0; i < 256; i++)
                hmac += rnd.Next(0, 2).ToString();

            return hmac;
        }

        internal void MakeSecure(string directory, decimal start, decimal inc, decimal end, int testbench, VerilogOutputType vot, bool printstats)
        {
            int j = 1;
            keyBits = 0;
            string circuit_file_secure = directory + CKT_NAME + "_000pObf_s.v";
            maxLUTID = int.Parse(CKT_LUTS[CKT_LUTS.Count - 1].LUTID);
            CalculateWeight();
            CKT_LUTS = CKT_LUTS.OrderByDescending(o => o.Weight).ToList();

            List<LUT> orig_luts = new List<LUT>(CKT_LUTS.Select(old => new LUT(old)).ToList());
            List<string> orig_wires = new List<string>(CKT_WIRES);

            CKT_LUTS = CKT_LUTS.OrderBy(o => int.Parse(o.LUTID)).ToList();

            WriteVerilog(circuit_file_secure, vot);
            //WriteBench(circuit_file_secure);


            PartitionedLUTs.Add(CKT_LUTS);

            if (embed_watermark) HMAC = GenerateHMAC();

            for (decimal i = start; i <= end; i += inc, j++)
            {

                CKT_LUTS = orig_luts.Select(old => new LUT(old)).ToList();
                CKT_WIRES = new List<string>(orig_wires);


                this.obfuscationPercentage = i;
                //SARLockWholeCircuit1(CKT_INPUTS.Count, 6);
                //SARLockDarkSilicon();
                Random rnd = new Random();
                for (int k = 0; k < CKT_INPUTS.Count; k++)
                {
                    sprng_key += rnd.Next(0, 2);
                    keyBits++;
                }
                MakeSecure(directory);
                CKT_LUTS = CKT_LUTS.OrderByDescending(o => o.Weight).ToList();

                circuit_file_secure = directory + CKT_NAME + "_" + ((int)(i * 100)).ToString("000") + "pObf_s" + ".v";
                //circuit_file_secure = directory + CKT_NAME + "_" + "0001" + "pObf_s" + ".v";
                //WriteBench(circuit_file_secure);
                

                CKT_LUTS = CKT_LUTS.OrderBy(o => int.Parse(o.LUTID)).ToList();
                //WriteBitstream(circuit_file_secure.Replace(".v", ".dat"));


                var tmp = sprng_key.ToArray();
                Array.Reverse(tmp);
                sprng_key = new string(tmp);
                sprng_keys.Add(sprng_key);
                WriteVerilog(circuit_file_secure, vot);
                WriteBLIF(directory);
                if (testbench > 0) InstantiationTemplates.Add(GenerateInstantiationTemplate(j));
                if (printstats) PrintStats();
         
                SecretKeys[j-1] = keyBits;
                keyBits = 0;
                PartitionedLUTs.Clear();

                sprng_key = string.Empty;
            }

            if (testbench > 0)
            {
                GenerateTestBench(directory);
                GenerateSecTestBench(directory);
            }
        }

        internal void MakeSecure(string directory)
        {
            DateTime start = DateTime.Now;
            Util.WriteInfo("Securing BLIF file...", true);
            LUTsShareKey = true;
            IsSecure = true;

            // hmac watermark variables
            bool map_hmac = embed_watermark;
            string hmac_tmp = HMAC;
            int hmac_idx = 0;
            StreamWriter hmac_statistics = new StreamWriter(directory + String.Format("{0}_hmac_statistics.txt", CKT_NAME));

            if (embed_watermark)
            {
                hmac_statistics.WriteLine("HMAC Generated: {0}\n", hmac_tmp);
            }

            List<LUT> maxLUTs = new List<LUT>();
            foreach (LUT lut in CKT_LUTS)
            {
                if (lut.NumInputs >= MaxLUTSize)
                    maxLUTs.Add(lut);
            }

            foreach (LUT lut in maxLUTs)
            {
                if (CKT_LUTS.Contains(lut))
                    CKT_LUTS.Remove(lut);
            }

            Random rand = new Random();
            Random keybit = new Random();
            int tmp = (int)(CKT_LUTS.Count * obfuscationPercentage);
            int maxCutoff = CalculateSimilarity(tmp);
           // List<List<LUT>> keybitsLUTs = new List<List<LUT>>();
            PartitionLUTs(maxCutoff);


            // to keep track of key placement, want to minimize observability of key bits
            int[] keyIndexUsed = new int[128];
            int numMaxSizeLUT = LUTSizeCount[MaxLUTSize];
            
            for (int i = 0; i < LUTSizeCount.Length; i++)
            {
                LUTSizeCount[i] = 0;
            }

            //int keyIndex = 0;
            int obfuscationRange = 0;
            if (obfuscationPercentage == 1)
                obfuscationRange = PartitionedLUTs.Count;
            else
                obfuscationRange = PartitionedLUTs.Count - 1;

            for(int i = 0; i < obfuscationRange; i++)
            {
                int range = getInputRange(PartitionedLUTs[i]);
                int lutindex = rand.Next(0, range);
                string bit_val = keybit.Next(0, 2).ToString();
                string hmac_substring = "";

                if (embed_watermark)
                {
                    var contentsize = PartitionedLUTs[i][0].ContentSize;
                    if (contentsize > hmac_tmp.Length)
                    {
                        hmac_substring = hmac_tmp.PadRight(contentsize, '0');
                        hmac_tmp = "";
                        //map_hmac = false;
                    }
                    else
                    {
                        hmac_substring = hmac_tmp.Substring(0, contentsize);
                        hmac_tmp = hmac_tmp.Remove(0, contentsize);
                    }
                }

                for (int j = 0; j < PartitionedLUTs[i].Count; j++)
                {
                    if(PartitionedLUTs[i][j].NumInputs >= MaxLUTSize)
                        LUTSizeCount[PartitionedLUTs[i][j].NumInputs]++;
                    else
                    {
                        if (j == 0 && map_hmac)
                        {
                            PartitionedLUTs[i][j].AddWatermarkSubstring(bit_val, hmac_substring, keyBits, lutindex);
                            hmac_statistics.WriteLine("{0},{1},{2}", PartitionedLUTs[i][j].LUTID, lutindex, bit_val);
                            //hmac_statistics.WriteLine("HMAC Portion Written: {0}", hmac_substring);
                            //hmac_statistics.WriteLine("LUT-ID Mapped: {0}", PartitionedLUTs[i][j].LUTID);
                            //hmac_statistics.WriteLine("");

                            if (hmac_tmp == "") map_hmac = false;
                        }
                        else
                        {
                            PartitionedLUTs[i][j].AddKeyInput(bit_val, lutindex, keyBits);
                            LUTSizeCount[PartitionedLUTs[i][j].NumInputs]++;
                        }
                    }           
                }
                sprng_key += bit_val;
                keyBits++;

            }

            // refill LUTSizeCount that was not obfuscated
            for (int i = maxCutoff; i < CKT_LUTS.Count; i++)
            {
                LUTSizeCount[CKT_LUTS[i].NumInputs]++;
            }

            foreach(LUT lut in maxLUTs)
            {
                CKT_LUTS.Add(lut);
                LUTSizeCount[lut.NumInputs]++;
            }

            //LUTSizeCount[MaxLUTSize] += numMaxSizeLUT; // add the original number of MaxLUTSize LUTs back in.
            DateTime end = DateTime.Now;
            var diff = end - start;
           // SARLockDarkSilicon();
            Console.WriteLine("Done. ({0:0.000} s)", diff.TotalSeconds);
            hmac_statistics.Close();
        }

        internal int SARLockWholeCircuit1(int numinputs, int maxlutsize)
        {
            int totalLUTS = 0;
            int MAXLUTSIZE = maxlutsize;
            int lutnum = maxLUTID + 1;
            int numSarlockKeys = numinputs;
            string[] SARLockKeys = new string[numSarlockKeys];
            string[] SARLockKeyVals = new string[numSarlockKeys];
            Dictionary<string, string> keyDictionary = new Dictionary<string, string>();
            string correctKey = "";
            Random rnd1 = new Random();
            List<string> ckt_inputs = new List<string>(CKT_INPUTS);
            List<string> lut_inputs = new List<string>(ckt_inputs);

            Random rnd = new Random();
            for (int i = 0; i < numSarlockKeys; i++)
            {
                SARLockKeys[i] = "sk[" + keyBits++ + "]";
                var keyval = rnd.Next(0, 2).ToString();
                correctKey += keyval;
                sprng_key += keyval;
                SARLockKeyVals[i] = keyval;
                lut_inputs.Insert(rnd.Next(0, lut_inputs.Count + 1), SARLockKeys[i]);
                keyDictionary.Add(SARLockKeys[i], keyval);
            }
            List<string> SATkeys = SARLockKeys.ToList();
            List<LUT> compare_LUTs = new List<LUT>();
            List<string> compareWires = new List<string>();

            int sizeSatLUT = MAXLUTSIZE / 2;
            int inputsCompare = numSarlockKeys;
            int compareIndex = 0;

            int numMaxLUTs = inputsCompare / sizeSatLUT;
            int numLeftover = inputsCompare % sizeSatLUT;


            for (int i = 0, j = 0; i < numMaxLUTs + (numLeftover > sizeSatLUT ? 2 : 1); i++, j += sizeSatLUT)
            {
                // numInputs is either the max size SAT LUT or the remaining number of inputs to compare
                List<string> lutInputs = new List<string>();
                int numInputs = j + sizeSatLUT > inputsCompare ? inputsCompare - j : sizeSatLUT ;
                string outputwire = "satCompare" + compareIndex++.ToString();

                // add 3 inputs, and 3 key bits to inputs list
                lutInputs.AddRange(ckt_inputs.GetRange(j, numInputs));
                lutInputs.AddRange(SATkeys.GetRange(j, numInputs));

                // create satlut
                LUT compareLUT = new LUT(outputwire, lutnum++.ToString(), lutInputs.ToArray());
                compareLUT.TruthTable = GenMinterms(compareLUT.NumInputs);

                //compareLUT.TruthTable = GenMinterms(MAXLUTSIZE);
                compare_LUTs.Add(compareLUT);
                compareWires.Add(outputwire);
                CKT_WIRES.Add(outputwire);

                if (compareLUT.NumInputs > 1)
                    totalLUTS++;
            }

            int numCompareWires = compare_LUTs.Count;
            int compareLevelWires = 0;
            int level = 0;
            bool levelsDone = false;

            while(!levelsDone)
            {
                // fill in logic levels to compress input/key comparison circuit
                int numLevelXLuts = compareWires.Count / MAXLUTSIZE;
                int numLevelLeftOver = compareWires.Count % MAXLUTSIZE;
                List<string> levelWires = new List<string>();

                for (int i = 0, j = 0; i < numLevelXLuts + (numLevelLeftOver > 0 ? 1 : 0); i++, j += MAXLUTSIZE)
                {
                    int numInputs = j + MAXLUTSIZE > compareWires.Count ? compareWires.Count - j : MAXLUTSIZE;
                    List<string> lutInputs = new List<string>(compareWires.GetRange(j, numInputs));
                    string wireOutput = "compareLevel" + level.ToString() + "out" + compareLevelWires++.ToString();

                    LUT compareLUT = new LUT(wireOutput, lutnum++.ToString(), lutInputs.ToArray());
                    compareLUT.TruthTable.Add("".PadLeft(numInputs, '1'), "1");
                    compare_LUTs.Add(compareLUT);
                    //CKT_LUTS.Add(compareLUT);
                    levelWires.Add(wireOutput);
                    CKT_WIRES.Add(wireOutput);
                    if (compareLUT.NumInputs > 1)
                        totalLUTS++;
                }

                compareWires = levelWires;
                compareLevelWires = 0;
                level++;

                if (levelWires.Count < MAXLUTSIZE)
                    levelsDone = true;
            }


            // Compare all key bits to mask correct key value
            List<LUT> maskLUTs = new List<LUT>();
            List<string> maskWires = new List<string>();
            int maskIndex = 0;

            numMaxLUTs = numSarlockKeys / MAXLUTSIZE;
            numLeftover = numSarlockKeys % MAXLUTSIZE;

            for(int i = 0, j = 0; i < numMaxLUTs + (numLeftover > 0 ? 1 : 0); i++, j += MAXLUTSIZE)
            {
                int numInputs = j + MAXLUTSIZE > numSarlockKeys ? numSarlockKeys - j : MAXLUTSIZE;

                List<string> lutInputs = new List<string>(SATkeys.GetRange(j, numInputs));
                string wireOutput = "maskWire" + maskIndex++.ToString();

                LUT maskLUT = new LUT(wireOutput, lutnum++.ToString(), lutInputs.ToArray());

                maskLUT.TruthTable.Add(correctKey.Substring(j, numInputs), "1");
                maskLUTs.Add(maskLUT);
                maskWires.Add(wireOutput);
                CKT_WIRES.Add(wireOutput);
                if (maskLUT.NumInputs > 1)
                    totalLUTS++;
            }

            bool maskLevelsDone = false;
            numCompareWires = maskLUTs.Count;
            int maskLevelWires = 0;
            level = 0;

            while (!maskLevelsDone)
            {
                // fill in logic levels to compress input/key comparison circuit
                int numLevelXLuts = maskWires.Count / MAXLUTSIZE;
                int numLevelLeftOver = maskWires.Count % MAXLUTSIZE;
                List<string> levelWires = new List<string>();

                for (int i = 0, j = 0; i < numLevelXLuts + (numLevelLeftOver > 0 ? 1 : 0); i++, j += MAXLUTSIZE)
                {
                    int numInputs = j + MAXLUTSIZE > maskWires.Count ? maskWires.Count - j : MAXLUTSIZE;
                    List<string> lutInputs = new List<string>(maskWires.GetRange(j, numInputs));
                    string wireOutput = "maskLevel" + level.ToString() + "out" + maskLevelWires++.ToString();

                    LUT compareLUT = new LUT(wireOutput, lutnum++.ToString(), lutInputs.ToArray());
                    compareLUT.TruthTable.Add("".PadLeft(numInputs, '1'), "1");
                    maskLUTs.Add(compareLUT);
                    //CKT_LUTS.Add(compareLUT);
                    levelWires.Add(wireOutput);
                    CKT_WIRES.Add(wireOutput);
                    if (compareLUT.NumInputs > 1)
                        totalLUTS++;
                }

                maskWires = levelWires;
                maskLevelWires = 0;
                level++;

                if (maskWires.Count == 1)
                    maskLevelsDone = true;
            }
            List<string> sentinelInputs = new List<string>(compareWires);
            sentinelInputs.Add(maskWires[0]);
            LUT sentinelLUT = new LUT("satwreckOut", lutnum++.ToString(), sentinelInputs.ToArray());
            sentinelLUT.TruthTable.Add("".PadLeft(compareWires.Count, '1') + '0', "1");
            CKT_LUTS.AddRange(compare_LUTs);
            CKT_LUTS.AddRange(maskLUTs);
            CKT_LUTS.Add(sentinelLUT);
            CKT_WIRES.Add("satwreckOut");
            totalLUTS++;
            // XOR all primary outputs with SARLock flip/mask unit
            foreach (LUT lut in CKT_LUTS)
            {
                if (!CKT_OUTPUTS.Contains(lut.LUToutput.Replace('[', '~').Replace(']','~'))) continue;
                //if (lut.NumInputs >= MAXLUTSIZE) continue;

                lut.LUTinputs.Insert(0, "satwreckOut");

                Dictionary<string, string> minterms = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in lut.TruthTable)
                {
                    string minterm = "0" + kvp.Key;
                    minterms.Add(minterm, kvp.Value);
                }

                lut.TruthTable = minterms;
                lut.NumInputs++;
                lut.ContentSize *= 2;
            }

            this.IsSecure = true;
            return totalLUTS;
        }

        internal Dictionary<string, string> GenMinterms(int lutsize)
        {
            Dictionary<string, string> minterms = new Dictionary<string, string>();

            for(int i = 0; i < Math.Pow(2, lutsize / 2); i++)
            {
                string tt_line = Convert.ToString(i, 2).PadLeft(lutsize / 2, '0');

                tt_line += tt_line;
                minterms.Add(tt_line, "1");
            }


            return minterms;
        }

        internal void SARLockWholeCircuit()
        {
            int lutnum = maxLUTID + 1;
            int numSarlockKeys = CKT_INPUTS.Count;
            string[] SARLockKeys = new string[numSarlockKeys];
            string[] SARLockKeyVals = new string[numSarlockKeys];
            Dictionary<string, string> keyDictionary = new Dictionary<string, string>();
            string correctKey = "";
            Random rnd1 = new Random();
            List<string> ckt_inputs = new List<string>(CKT_INPUTS.OrderBy(x => rnd1.Next()));
            List<string> lut_inputs = new List<string>(ckt_inputs);

            Random rnd = new Random();
            for(int i = 0; i < numSarlockKeys; i++)
            {
                SARLockKeys[i] = "sk[" + keyBits++ + "]";
                var keyval = rnd.Next(0, 2).ToString();
                correctKey += keyval;
                sprng_key += keyval;
                SARLockKeyVals[i] = keyval;
                lut_inputs.Insert(rnd.Next(0, lut_inputs.Count + 1), SARLockKeys[i]);
                keyDictionary.Add(SARLockKeys[i], keyval);
            }

            string correctKeytmp = correctKey;
            List<string> SATkeys = SARLockKeys.ToList();
            List<LUT> compare_LUTs = new List<LUT>();
            List<string> compareWires = new List<string>();

            int sizeSatLUT= MaxLUTSize / 2;
            int inputsCompare = numSarlockKeys;
            int compareIndex = 0;

            while(inputsCompare > 0)
            {
                int numLUTinputs;
                List<string> lutInputs = new List<string>();
                if (inputsCompare >= sizeSatLUT)
                {
                    numLUTinputs = sizeSatLUT;
                    inputsCompare -= sizeSatLUT;
                }
                else
                {
                    // reach endcase - no more inputs left to compare
                    numLUTinputs = inputsCompare;
                    inputsCompare = 0;
                }

                List<string> sarlockKeys = new List<string>();
                string cur_correctKey = "";
                for(int i = 0; i < numLUTinputs; i++)
                {
                    sarlockKeys.Add(SATkeys[0]);
                    SATkeys.RemoveAt(0);
                }

                cur_correctKey = correctKeytmp.Substring(0, numLUTinputs);
                correctKeytmp = correctKeytmp.Substring(numLUTinputs - 1, correctKeytmp.Length - numLUTinputs);
                Random rng1 = new Random();

                for(int i = 0; i < numLUTinputs; i++)
                {
                    int index = rng1.Next(0, ckt_inputs.Count);
                    lutInputs.Add(ckt_inputs[index]);
                    ckt_inputs.RemoveAt(index);
                }

                List<string> circuit_inputs = new List<string>(lutInputs);
                Random rng2 = new Random();
                lutInputs.AddRange(sarlockKeys);
                lutInputs.OrderBy(x => rng2.Next());

                string outputName = "satCompare" + compareIndex++.ToString();
                compareWires.Add(outputName);
                LUT inputLUT = new LUT(outputName, lutnum++.ToString(), lutInputs.ToArray());

                Dictionary<string, int> inputPositions = new Dictionary<string, int>();
                for (int i = 0; i < inputLUT.NumInputs; i++)
                    inputPositions.Add(inputLUT.LUTinputs[i], i);

                Dictionary<string, string> newMinterms = new Dictionary<string, string>();

                for (int i = 0; i < Math.Pow(2, numLUTinputs); i++)
                {
                    string tt_line = Convert.ToString(i, 2).PadLeft(numLUTinputs, '0');

                    if (tt_line != "")
                    {
                        char[] minterm = new char[inputLUT.NumInputs];
                        for (int j = 0; j < numLUTinputs; j++)
                        {
                            minterm[inputPositions[circuit_inputs[j]]] = tt_line[j];
                        }
                        for (int j = 0; j < sarlockKeys.ToList().Count; j++)
                        {
                            minterm[inputPositions[sarlockKeys[j]]] = tt_line[j];
                        }
                        newMinterms.Add(new string(minterm), "1");
                    }
                }
                inputLUT.TruthTable = newMinterms;
                compare_LUTs.Add(inputLUT);
            }

            int level1wires = compareWires.Count;

            List<LUT> compare_level1LUTs = new List<LUT>();
            List<string> compareWires_tmp = new List<string>(compareWires);
            List<string> compare_level1Wires = new List<string>();
            int compare_level1Wire = 0;
            while (level1wires > 0)
            {
                int numlutinputs = 0;
                if(level1wires >= MaxLUTSize)
                {
                    numlutinputs = MaxLUTSize;
                    level1wires -= MaxLUTSize;
                }
                else
                {
                    numlutinputs = level1wires;
                    level1wires = 0;
                }

                List<string> level1inputs = new List<string>(compareWires_tmp.GetRange(0, numlutinputs));
                //level1inputs.AddRange(compareWires.GetRange(0, numlutinputs));
                compareWires_tmp = compareWires_tmp.GetRange(numlutinputs, compareWires_tmp.Count - numlutinputs);

                string compareWire = "groupCompare" + compare_level1Wire++.ToString();
                compare_level1Wires.Add(compareWire);
                LUT level1LUT = new LUT(compareWire, lutnum++.ToString(), level1inputs.ToArray());
                Dictionary<string, string> newMinterms = new Dictionary<string, string>();
                newMinterms.Add("".PadLeft(level1inputs.Count, '1'), "1");

                level1LUT.TruthTable = newMinterms;
                compare_level1LUTs.Add(level1LUT);
            }


            List<LUT> maskLUTs = new List<LUT>();


            //LUT inputLut = new LUT("sarlockOut", "0", lut_inputs.ToArray());
            List<string> maskWires = new List<string>();
            int keybitsCompare = numSarlockKeys;
            int keyindex = 0;
            int maskIndex = 0;
            while(keybitsCompare > 0)
            {
                int numlutinputs = 0;
                if (keybitsCompare >= MaxLUTSize)
                {
                    numlutinputs = MaxLUTSize;
                    keybitsCompare -= MaxLUTSize;
                }
                else
                {
                    numlutinputs = keybitsCompare;
                    keybitsCompare = 0;
                }

                List<string> inputsList = new List<string>();
                string correctKeyval = "";
                for (int i = keyindex; i < keyindex + numlutinputs; i++)
                {
                    inputsList.Add(SARLockKeys[i]);
                    correctKeyval += SARLockKeyVals[i];
                }
                keyindex += numlutinputs;
                string maskLUTOutput = "keymask" + maskIndex++.ToString();
                maskWires.Add(maskLUTOutput);
                LUT maskLUT = new LUT(maskLUTOutput, lutnum++.ToString(), inputsList.ToArray());

                maskLUT.TruthTable.Add(correctKeyval, "1");
                maskLUTs.Add(maskLUT);
            }

            LUT maskSentinel = new LUT("maskSignal", lutnum++.ToString(), maskWires.ToArray());
            maskSentinel.TruthTable.Add("".PadLeft(maskWires.Count, '1'), "1");
            maskLUTs.Add(maskSentinel);


            List<string> sat_circuit_inputs = new List<string>();
            sat_circuit_inputs.AddRange(compare_level1Wires);
            sat_circuit_inputs.Add("maskSignal");
            LUT finalLUT = new LUT("sarlockOut", lutnum++.ToString(), sat_circuit_inputs.ToArray());

            finalLUT.TruthTable.Add("".PadLeft(compare_level1Wires.Count, '1') + "0", "1");

            CKT_LUTS.AddRange(compare_LUTs);
            CKT_LUTS.AddRange(compare_level1LUTs);
            CKT_LUTS.AddRange(maskLUTs);
            CKT_LUTS.Add(finalLUT);

            CKT_WIRES.AddRange(compareWires);
            CKT_WIRES.AddRange(compare_level1Wires);
            CKT_WIRES.AddRange(maskWires);
            CKT_WIRES.Add("maskSignal");
            //CKT_WIRES.Add("sarlockOut");


            CKT_WIRES.Add("sarlockOut");
            //Dictionary<string, int> inputPositions = new Dictionary<string, int>();
            //for (int i = 0; i < inputLut.NumInputs; i++)
            //    inputPositions.Add(inputLut.LUTinputs[i], i);

            //Dictionary<string, string> newMinterms = new Dictionary<string, string>();

            //int flip_count = 0;
            //for (int i = 0; i < Math.Pow(2, numSarlockKeys); i++)
            //{
            //    string tt_line = Convert.ToString(i, 2).PadLeft(numSarlockKeys, '0');

            //    if(tt_line != correctKey)
            //    {
            //        char[] minterm = new char[inputLut.NumInputs];
            //        for (int j = 0; j < ckt_inputs.Count; j++)
            //        {
            //            minterm[inputPositions[ckt_inputs[j]]] = tt_line[j];
            //        }
            //        for (int j = 0; j < SARLockKeys.ToList().Count; j++)
            //        {
            //            minterm[inputPositions[SARLockKeys[j]]] = tt_line[j];
            //        }
            //        newMinterms.Add(new string(minterm), "1");
            //    }
            //}

            //inputLut.TruthTable = newMinterms;
            //LUTSizeCount[inputLut.NumInputs]++;
            //CKT_LUTS.Add(inputLut);

            // XOR all primary outputs with SARLock flip/mask unit
            foreach (LUT lut in CKT_LUTS)
            {
                if (!CKT_OUTPUTS.Contains(lut.LUToutput)) continue;
                //if (lut.NumInputs >= MaxLUTSize) continue;

                lut.LUTinputs.Insert(0, "sarlockOut");

                Dictionary<string, string> minterms = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in lut.TruthTable)
                {
                    string minterm = "0" + kvp.Key;
                    minterms.Add(minterm, kvp.Value);
                }

                lut.TruthTable = minterms;
                lut.NumInputs++;
                lut.ContentSize *= 2;
            }

            this.IsSecure = true;
        }

        internal void SARLockDarkSilicon()
        {
            int count = 0;
            foreach(LUT lut in CKT_LUTS)
            {
                if (count > 0) continue;
                //if (lut.NumInputs < 14) continue;
                //if (count > 0) break;
                //count++;
                //if (!lut.secured) continue;
                //if (!CKT_OUTPUTS.Contains(lut.LUToutput) && lut.Weight < 1500) continue;

                //if (CKT_OUTPUTS.Contains(lut.LUToutput) && lut.NumInputs == 1) continue;
                //if (lut.Weight < 1500) continue;


                int numInputsCompare = lut.NumInputs;// lut.NumInputs - 1;
                //if (!lut.secured)
                //    numInputsCompare = 1;
                //else
                //    numInputsCompare = lut.NumInputs - 1;

                //if (lut.NumInputs == 12)
                //{
                //    numInputsCompare = lut.NumInputs - 4;
                //}
                //else if (lut.NumInputs == 11)
                //    numInputsCompare = lut.NumInputs - 3;
                //else if (lut.NumInputs == 10)
                //    numInputsCompare = lut.NumInputs - 2;
                //else
                //if (lut.NumInputs == 16 && count == 0)
                //{
                //    numInputsCompare = lut.NumInputs;
                //    count++;
                //}
                //else
                //    continue;

                LUTSizeCount[lut.NumInputs]--;

                // values for new key inputs, as well as MUTARCH Key
                string Km = "sk[" + lut.keyIndex + "]";                 // MUTARCH key index
                string Km_val = lut.keyVal;                             // MUTARCH key value
                string[] SARLockKeys = new string[numInputsCompare];    // Array to store SARLock keys inserted to LUT
                string[] SARLockKeyVals = new string[numInputsCompare]; // Array to store SARLock key values
                string correctKey = "";
                // end values

                // assign key indices
                for (int i = 0; i < SARLockKeys.Length; i++)
                    SARLockKeys[i] = "sk[" + keyBits++ + "]";

                // generate key vals
                Random rnd1 = new Random();
                for (int i = 0; i < SARLockKeyVals.Length; i++)
                {
                    string keyval = rnd1.Next(0, 2).ToString();
                    SARLockKeyVals[i] = keyval;
                    sprng_key += keyval;
                }

                //// match value for our flip generation
                //string match = Km_val == SARLockKeyVals[0] ? "0" : "1";
                //for (int i = 1; i < numInputsCompare; i++)
                //    match += SARLockKeyVals[i];

                // store the correct key value
                if(lut.secured)
                    correctKey = Km_val;

                for (int i = 0; i < numInputsCompare; i++)
                    correctKey += SARLockKeyVals[i];

                // all inputs into LUT 
                List<string> inputs = new List<string>();
                List<string> inputsWithKey = new List<string>(lut.LUTinputs);
                foreach(string input in lut.LUTinputs)
                    if (input != Km) inputs.Add(input);


                Dictionary<string, int> inputPositions = new Dictionary<string, int>();
                Random rnd = new Random();
                // insert SARLock key bits into LUT inputs, keep track of positions
                for (int i = 0; i < numInputsCompare; i++)
                {
                    
                    int position = rnd.Next(0, lut.NumInputs + 1);
                    //inputPositions.Add(SARLockKeys[i], position);
                    lut.LUTinputs.Insert(position, SARLockKeys[i]);
                    lut.NumInputs++;
                }

                for (int i = 0; i < lut.NumInputs; i++)
                    inputPositions.Add(lut.LUTinputs[i], i);

                // keep track of all input positions
                for(int i = 0; i < lut.LUTinputs.Count; i++)
                {
                    if (!inputPositions.ContainsKey(lut.LUTinputs[i])) inputPositions.Add(lut.LUTinputs[i], i);
                }

                Dictionary<string, string> newMinterms = new Dictionary<string, string>();
                int flip_count = 0;
                Parallel.For(0, (int)Math.Pow(2, lut.NumInputs), i =>
                {
                    bool flip = false;
                    bool mask = false;
                    string tt_line = Convert.ToString(i, 2).PadLeft(lut.NumInputs, '0');

                    string key = "";
                    if (lut.secured)
                        key = tt_line[inputPositions[Km]].ToString();

                    string pattern = "";
                    string allInputsPattern = "";

                    // match = (Km ^ Ks0)
                    string match = "";
                    if (lut.secured)
                        match = tt_line[inputPositions[SARLockKeys[0]]] == tt_line[inputPositions[Km]] ? "0" : "1";

                    // match = (Km ^ Ks0) AND Ks1 AND Ks2 AND ... Ksn
                    if (lut.secured)
                    {
                        for (int j = 1; j < numInputsCompare; j++)
                        {
                            match += tt_line[inputPositions[SARLockKeys[j]]].ToString();
                        }
                    }
                    else
                    {
                        for (int j = 0; j < numInputsCompare; j++)
                        {
                            match += tt_line[inputPositions[SARLockKeys[j]]].ToString();
                        }
                    }


                    // input pattern used for comparing to match sequence
                    for (int j = 0; j < numInputsCompare; j++)
                    {
                        if (inputs[j] == Km) continue;
                        pattern += tt_line[inputPositions[inputs[j]]].ToString();
                    }

                    // get minterm of all inputs including MUTARCH key, to compare to original TT
                    for (int j = 0; j < inputsWithKey.Count; j++)
                    {
                        allInputsPattern += tt_line[inputPositions[inputsWithKey[j]]].ToString();
                    }

                    // key value of Km Ks0 ... Ksn to see if we have the correct key
                    for (int j = 0; j < numInputsCompare; j++)
                        key += tt_line[inputPositions[SARLockKeys[j]]].ToString();

                    if (pattern == match)
                    {
                        //flip_count++;
                        flip = true;
                    }

                    if (key == correctKey)
                    {
                        mask = true;
                    }

                    //if (flip && !mask)


                    if (mask)
                    {
                        // if current expression matches original minterm, include in new TT
                        if (matchTT(allInputsPattern, lut.TruthTable) == "1")
                            newMinterms.Add(tt_line, "1");
                    }
                    else if (flip)
                    {
                        // if current expression matches a minterm from original TT, don't include it, if output would originally be 0, flip it and include new minterm
                        if (matchTT(allInputsPattern, lut.TruthTable) == "0")
                            newMinterms.Add(tt_line, "1");

                        flip_count++;
                    }
                    else
                    {
                        if (matchTT(allInputsPattern, lut.TruthTable) == "1")
                            newMinterms.Add(tt_line, "1");
                    }

                }
                );

                List<string> incorrectKeys = getIncorrectKeys(correctKey);


                ////// MY ATTEMPT TO SPEED THIS UP FOR THE LOVE OF GOD IT'S SO SLOW!!!!
                //for (int i = 0; i < numInputsCompare; i++)
                //{
                //    bool flip = false;
                //    bool mask = false;
                //    string tt_line = Convert.ToString(i, 2).PadLeft(numInputsCompare, '0');


                //    string keypattern = tt_line;


                //    if (keypattern == correctKey)
                //    {
                //        mask = true;
                //    }

                //    AddIncorrectKeys(tt_line, correctKey, incorrectKeys, inputs, SARLockKeys.ToList(), inputPositions, lut.TruthTable, ref newMinterms);

                //    if (mask)
                //    {
                //        // if current expression matches original minterm, include in new TT
                //        if (matchTT(keypattern, lut.TruthTable) == "1")
                //            newMinterms.Add(tt_line, "1");
                //    }
                //    else if (flip)
                //    {
                //        // if current expression matches a minterm from original TT, don't include it, if output would originally be 0, flip it and include new minterm
                //        if (matchTT(keypattern, lut.TruthTable) == "0")
                //            newMinterms.Add(tt_line, "1");

                //        flip_count++;
                //    }
                //    else
                //    {
                //        if (matchTT(keypattern, lut.TruthTable) == "1")
                //            newMinterms.Add(tt_line, "1");
                //    }
                //}
                count++;

                lut.TruthTable = newMinterms;
                lut.MinTruthTable = newMinterms;
                lut.ContentSize = (int)Math.Pow(2.0, (double)lut.NumInputs);
                LUTSizeCount[lut.NumInputs]++;
                this.IsSecure = true;
            }
        }

        private void AddIncorrectKeys(string pattern, string correctKey, List<string> incorrectKeyPatterns, List<string> inputs, List<string> keybits, Dictionary<string, int> inputPositions, Dictionary<string, string> minterms, ref Dictionary<string, string> newMinterms)
        {
            if(matchTT(pattern, minterms) == "1")
            {
                foreach(string keypattern in incorrectKeyPatterns)
                {
                    bool isMatch = true;
                    //if()
                }
            }
        }

        private string buildMinterm(string inputPattern, string keyPattern, List<string> inputs, List<string> keybits, Dictionary<string, int> inputPositions, Dictionary<string, string> minterms)
        {
            string ret = "";
            string tt_output_val = matchTT(inputPattern, minterms);
            

            return ret;
        }

        private List<string> getIncorrectKeys(string correctKey)
        {
            List<string> ret = new List<string>();
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < correctKey.Length; i++)
            {
                sb.Append("".PadLeft(i, '-'));
                sb.Append(correctKey[i] == '0' ? "1" : "0");
                sb.Append("".PadRight(correctKey.Length - i - 1, '-'));
                ret.Add(sb.ToString());
                sb.Clear();
            }

            return ret;
        }

        private string matchTT(string pattern, Dictionary<string, string> tt)
        {
            string ret = "0";

            foreach(KeyValuePair<string, string> tt_entry in tt)
            {
                bool isMatch = true;

                for (int i = 0; i < tt_entry.Key.Length; i++)
                {
                    if (tt_entry.Key[i] == '-') continue;
                    if (tt_entry.Key[i] != pattern[i]) isMatch = false;
                }

                if (isMatch)
                {
                    ret = tt_entry.Value;
                    break;
                }
            }
            return ret;
        }

        internal string[] ScrambleInputs(string[] inputs_orig)
        {
            Random rnd = new Random();

            for(int i = inputs_orig.Length; i > 0; i--)
            {
                int j = rnd.Next(i);
                string k = inputs_orig[j];
                inputs_orig[j] = inputs_orig[i - 1];
                inputs_orig[i - 1] = k;
            }
            return inputs_orig;
        }

        internal int getInputRange(List<LUT> chk)
        {
            int ret = 10000000;
            for(int i = 0; i < chk.Count; i++)
            {
                if (chk[i].NumInputs < ret)
                    ret = chk[i].NumInputs;
            }
            return ret;
        }

        // generate similarity list for all LUTs subject to obfuscation
        internal int CalculateSimilarity(int maxCutoff)
        {
            DateTime start = DateTime.Now;
            Util.WriteInfo("Calculating LUT similarity...", true);
            int ret = maxCutoff;
            int test = 0;
            // calculate overall similarity
            for (int i = 0; i < ret; i++)
            {
                for (int j = 0; j < ret; j++)
                {
                    if (i >= ret) continue;
                    if (i == j || CKT_LUTS[j].similarityList.Count > 0) continue;

                    // # inputs shared between LUTs
                    double sharedCount = CKT_LUTS[i].LUTinputs.Intersect(CKT_LUTS[j].LUTinputs).Count();
                    double inputSize = (CKT_LUTS[i].NumInputs + CKT_LUTS[j].NumInputs) / 2;
                    if(sharedCount > 0)
                    {
                        avgSimilarity += (sharedCount / inputSize);
                        test++;
                    }
                }
            }
            avgSimilarity = avgSimilarity / (/*(test * test) - */test); //((ret * ret) - ret);
            avgSimilarity = Math.Floor(avgSimilarity * 10) / 10;

            DateTime end = DateTime.Now;
            var diff = end - start;
            Console.WriteLine("Done. ({0:0.000} s)", diff.TotalSeconds);
            return ret;
        }


        // calculate weight of a LUT based on fanout into other LUTs
        internal void CalculateWeight()
        {
            DateTime start = DateTime.Now;
            Util.WriteInfo("Calculating LUT Weight...", true);

            for (int i = 0; i < CKT_LUTS.Count; i++)
            {
                // do not consider Max-input LUTS for obfuscation
                if (CKT_LUTS[i].NumInputs >= MaxLUTSize)
                    continue;

                //if (CKT_LUTS[i].NumInputs == 6)
                //    CKT_LUTS[i].Weight += 40;

                //if (CKT_LUTS[i].LUTinputs.Intersect(this.CKT_INPUTS).Count() == CKT_LUTS[i].NumInputs)
                //    CKT_LUTS[i].Weight += 150;

                //if (CKT_OUTPUTS.Contains(CKT_LUTS[i].LUToutput))
                //    CKT_LUTS[i].Weight += 1500;

                //CKT_LUTS[i].Weight += CKT_LUTS[i].LUTinputs.Intersect(CKT_INPUTS).Count();

                for (int j = 0; j < CKT_LUTS.Count; j++)
                {
                    if (i == j) continue;

                    // Weight determined by # of inputs driven by it's output 
                    if (CKT_LUTS[j].LUTinputs.Contains(CKT_LUTS[i].LUToutput))
                        CKT_LUTS[i].Weight++;
                    

                    //if (CKT_OUTPUTS.Contains(CKT_LUTS[j].LUToutput) && CKT_LUTS[j].LUTinputs.Contains(CKT_LUTS[i].LUToutput) || CKT_LUTS[i].NumInputs == 6)
                    //    CKT_LUTS[i].Weight += 1500;
                }
            }

            DateTime end = DateTime.Now;
            var diff = end - start;
            Console.WriteLine("Done. ({0:0.000} s)", diff.TotalSeconds);
        }

        public void PartitionLUTs(int maxCutoff)
        {
            if (maxCutoff < CKT_LUTS.Count)
            {
                List<LUT> temp = CKT_LUTS.GetRange(0, maxCutoff);
                PartitionedLUTs = LUT.PartitionLUTs(temp, avgSimilarity);
                PartitionedLUTs.Add(CKT_LUTS.GetRange(maxCutoff, CKT_LUTS.Count - maxCutoff));
            } else
                PartitionedLUTs = LUT.PartitionLUTs(CKT_LUTS, avgSimilarity);

        }

        public string GenerateInstantiationTemplate(int moduleIndex)
        {
            StringBuilder sb = new StringBuilder();

            string goldenModule = CKT_NAME + "_000pObf_s";
            string obfuscatedModule = CKT_NAME + "_" + ((int)(obfuscationPercentage * 100)).ToString("000") + "pObf_s";
            Dictionary<string, int> input_vectors = parseVectors(CKT_INPUTS);
            Dictionary<string, int> output_vectors = parseVectors(CKT_OUTPUTS);
            Dictionary<string, int> testInputVectors = new Dictionary<string, int>();
            Dictionary<string, int> testOutputVectors = new Dictionary<string, int>();

            int inputCount = 0;
            int outputCount = 0;
            bool buses = false;

            foreach (KeyValuePair<string, int> vector in input_vectors)
            {
                if (vector.Value > 1)
                {
                    buses = true;
                    testInputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    inputCount++;
                }
            }


            if (inputCount > 0 && !buses) testInputVectors.Add("inputs", inputCount - 1);

            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                if (vector.Value > 1)
                {
                    testOutputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    outputCount++;
                }
            }

            if (outputCount > 0) testOutputVectors.Add("outputs", outputCount - 1);

            sb.AppendFormat("{0} {0} (.sk(sk{2}), ", obfuscatedModule, (obfuscationPercentage * 100).ToString("000"), moduleIndex);

            int inputIndex = 0;
            foreach (KeyValuePair<string, int> vector in input_vectors)
            {
                if (buses)
                {
                    sb.AppendFormat(".{0}({0}), ", vector.Key);
                }
                /*
                if (testInputVectors.Contains(vector))
                {
                    sb.AppendFormat(".{0}({0}), ", vector);
                }
                */
                else
                {
                    sb.AppendFormat(".{0}(inputs[{1}]), ", vector.Key, inputIndex++);
                }
            }

            int outputIndex = 0;
            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                if (buses)
                {
                    if (vector.Value > 0)
                    {
                        if (vector.Key == output_vectors.Last().Key)
                            sb.AppendFormat(".{0}({0}{1}));", vector.Key, moduleIndex);
                        else
                            sb.AppendFormat(".{0}({0}{1}), ", vector.Key, moduleIndex);
                    }
                }
                else
                {
                    if (vector.Key == output_vectors.Last().Key)
                    {
                        sb.AppendFormat(".{0} (obfusoutputs{2}[{1}]));", vector.Key, outputIndex++, moduleIndex);
                    }

                    else
                        sb.AppendFormat(".{0}(obfusoutputs{2}[{1}]), ", vector.Key, outputIndex++, moduleIndex);
                }

                /*
                if (testOutputVectors.Contains(vector))
                {
                    if (vector.Key == output_vectors.Last().Key)
                    {
                        sb.AppendFormat(".{0} (outputs{2}[{1}]));", vector.Key, outputIndex++, moduleIndex);
                    }

                    else
                        sb.AppendFormat(".{0}(outputs{2}[{1}]), ", vector.Key, outputIndex++, moduleIndex);
                }
                else
                {
                    if (vector.Key == output_vectors.Last().Key)
                        sb.AppendFormat(".{0} (obfusoutputs{2}[{1}]));", vector.Key, outputIndex++, moduleIndex);
                    else
                        sb.AppendFormat(".{0}(obfusoutputs{2}[{1}]), ", vector.Key, outputIndex++, moduleIndex);
                }
                */
            }

            return sb.ToString();
        }

        internal void GenerateSecTestBench(string directory)
        {
            string goldenModule = CKT_NAME + "_000pObf_s";
            //string obfuscatedModule = bf.CKT_NAME + "_" + ((int)(bf.obfuscationPercentage * 100)).ToString("000") + "pObf_s";

            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> input_vectors = parseVectors(CKT_INPUTS);
            Dictionary<string, int> output_vectors = parseVectors(CKT_OUTPUTS);
            Dictionary<string, int> testInputVectors = new Dictionary<string, int>();
            Dictionary<string, int> testOutputVectors = new Dictionary<string, int>();

            int inputCount = 0;
            int outputCount = 0;

            foreach (KeyValuePair<string, int> vector in input_vectors)
            {
                if (vector.Value > 1)
                {
                    testInputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    inputCount++;
                }
            }

            if (inputCount > 0) testInputVectors.Add("inputs", inputCount - 1);

            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                if (vector.Value > 1)
                {
                    testOutputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    outputCount++;
                }
            }

            if (outputCount > 0) testOutputVectors.Add("outputs", outputCount - 1);


            StreamWriter tb = new StreamWriter(string.Format("{0}FunctionalMismatch{1}_s.v", directory, CKT_NAME));

            tb.WriteLine("/* Testbench auto-generated by EDA Tool */");
            tb.WriteLine("`timescale 10ns/1ns");
            tb.WriteLine("module GoldenChip_{0} ();\n", CKT_NAME);

            tb.Write("\n");

            tb.WriteLine("/* INPUTS */");

            int max_count = 0;
            int increment_amount = 0;

            foreach (KeyValuePair<string, int> vector in testInputVectors)
            {
                if (vector.Value > 1)
                {
                    var max_value = Math.Pow(2, vector.Value + 1);
                    if (max_value > 65536)
                    {
                        max_count = 65536;
                        increment_amount = (int)((max_value / max_count) - 1);
                    }
                    else
                    {
                        max_count = (int)max_value;
                        increment_amount = 1;
                    }

                    tb.WriteLine("reg [{0}:0] {1} = 0;", vector.Value, vector.Key);
                    tb.WriteLine("localparam MAX_COUNT = {0};", max_count);
                }
                else
                {
                    var max_value = Math.Pow(2, vector.Value + 1);
                    if (max_value > 65536)
                    {
                        max_count = 65536;
                        increment_amount = (int)((max_value / max_count) - 1);
                    }
                    else
                    {
                        max_count = (int)max_value;
                        increment_amount = 1;
                    }

                    tb.WriteLine("reg {0} = 0;", vector.Value);
                    tb.WriteLine("localparam MAX_COUNT = {0};", max_count);
                }
            }

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("reg [{0}:0] sk{1};", SecretKeys[i] - 1, i + 1);
            }

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("localparam [{0}:0] correct_sk{1} = {2}'b{3};", SecretKeys[i] - 1, i + 1, SecretKeys[i], sprng_keys[i]);
            }

            tb.WriteLine("/* OUTPUTS */");
            foreach (KeyValuePair<string, int> vector in testOutputVectors)
            {
                if (vector.Value > 1)
                {
                    tb.WriteLine("wire [{0}:0] {1};", vector.Value, vector.Key);
                    for (int i = 0; i < InstantiationTemplates.Count; i++)
                    {
                        tb.WriteLine("wire [{0}:0] {1}{2};", vector.Value, "obfus" + vector.Key, i + 1);
                    }

                    for (int i = 0; i < InstantiationTemplates.Count; i++)
                    {
                        tb.WriteLine("wire [{0}:0] mismatch{1};", vector.Value, i + 1);
                    }

                }
                // else
                //     tb.WriteLine("write {0};", vector.Value);
            }



            tb.WriteLine("/* END NETS */");
            tb.Write("\n");

            //// WRITE INSTANTIATION FOR GOLDEN MODULE ------------------------------------------------------------
            //tb.Write("{0} {0} (", goldenModule);

            //int inputIndex = 0;
            //foreach (KeyValuePair<string, int> vector in input_vectors)
            //{
            //    if (testInputVectors.Contains(vector))
            //    {
            //        tb.Write(".{0}({0}), ", vector.Value);
            //    }
            //    else
            //    {
            //        tb.Write(".{0}(inputs[{1}]), ", vector.Key, inputIndex++);
            //    }
            //}

            //tb.Write("\n");

            //int outputIndex = 0;
            //foreach (KeyValuePair<string, int> vector in output_vectors)
            //{
            //    if (testOutputVectors.Contains(vector))
            //    {
            //        if (vector.Key == output_vectors.Last().Key)
            //            tb.Write(".{0} (outputs[{1}]));", vector.Key, outputIndex++);
            //        else
            //            tb.Write(".{0}(outputs[{1}]), ", vector.Key, outputIndex++);
            //    }
            //    else
            //    {
            //        if (vector.Key == output_vectors.Last().Key)
            //            tb.Write(".{0} (outputs[{1}]));", vector.Key, outputIndex++);
            //        else
            //            tb.Write(".{0}(outputs[{1}]), ", vector.Key, outputIndex++);
            //    }
            //}

            //tb.Write("\n");
            //tb.Write("\n");
            //// END WRITE INSTANTIATION FOR GOLDEN MODULE ------------------------------------------------------------


            // WRITE INSTANTIATION FOR OBFUSCATED MODULE ------------------------------------------------------------
            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine(InstantiationTemplates[i]);
            }
            tb.Write("\n");
            tb.Write("\n");
            // END WRITE INSTANTIATION FOR OBFUSCATED MODULE ------------------------------------------------------------

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("assign mismatch{0} = outputs ^ obfusoutputs{0};", i + 1);
            }

            tb.Write("\n");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("real mismatchPercentage{0} = 0;", i + 1);

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("real keyPercentageCorrect{0} = 0;", i + 1);

            tb.Write("integer ");
            for (int i = 0; i < InstantiationTemplates.Count - 1; i++)
            {
                tb.Write("f{0}, ", i + 1);
            }
            tb.Write("f{0};\n", InstantiationTemplates.Count);

            tb.Write("integer ");
            for (int i = 0; i < InstantiationTemplates.Count - 1; i++)
            {
                tb.Write("rk{0}, ", i + 1);
            }
            tb.Write("rk{0};\n", InstantiationTemplates.Count);

            tb.WriteLine("integer i, j, k, l;");
            tb.WriteLine("initial begin");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("\trk{0} = $fopen(\"RNG1.txt\", \"r\");", i + 1);

            tb.WriteLine("\tsk1 = correct_sk1;");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                string []splitTemplate = InstantiationTemplates[i].Split('_');
                string percentage = splitTemplate[1].Substring(0, 3);
                tb.WriteLine("\tf{0} = $fopen(\"functionalMismatch_{1}_{2}.txt\", \"w\");", i + 1, CKT_NAME, percentage/*((i + 1) * 5).ToString("000")*/);
            }
                
            // tb.WriteLine("\tf = $fopen(\"functionalMismatch{0}.txt\", \"w\");", obfuscatedModule);
            //     for(int i = 0; i < InstantiationTemplates.Count)
            //tb.WriteLine("$fwrite(f, \"input, sk, mismatch\\n\");");

            tb.WriteLine("end");

            tb.WriteLine("initial begin");
            tb.WriteLine("\tfor(i = 0; i < MAX_COUNT; i = i +1) begin");

           // tb.WriteLine("\t\tfor(l = 0; l < 10; l = l + 1) begin");

           // for (int i = 0; i < InstantiationTemplates.Count; i++)
           //     tb.WriteLine("\t\t\t$fscanf(rk{0}, \"%{1}b\", sk{0});", i + 1, SecretKeys[i]);

           //// tb.WriteLine("\t\t\t#1;");

           // for (int i = 0; i < InstantiationTemplates.Count; i++)
           // {
           //     tb.WriteLine("\t\t\tfor(j = 0; j < {0}; j = j + 1) begin", testOutputVectors.ElementAt(0).Value + 1);
           //     tb.WriteLine("\t\t\t\tmismatchPercentage{0} = mismatchPercentage{0} + mismatch{0}[j];", i + 1);
           //     tb.WriteLine("\t\t\tend");
           // }
           // //tb.WriteLine("\t\t\tfor(j = 0; j < {0}; j = j + 1) begin", testOutputVectors.ElementAt(0).Value + 1);
           // //tb.WriteLine("\t\t\t\tmismatchPercentage = mismatchPercentage + mismatch[j];");
           // //tb.WriteLine("\t\t\tend");

           // for (int i = 0; i < InstantiationTemplates.Count; i++)
           // {
           //     tb.WriteLine("\t\t\tfor(k = 0; k < {0}; k = k + 1) begin", SecretKeys[i]);
           //     tb.WriteLine("\t\t\t\tkeyPercentageCorrect{0} = keyPercentageCorrect{0} + (sk{0}[k] == correct_sk{0}[k]);", i + 1);
           //     tb.WriteLine("\t\t\tend");
           // }
           // //tb.WriteLine("\t\t\tfor(k = 0; k < {0}; k = k + 1) begin", keyBits);
           // //tb.WriteLine("\t\t\t\tkeyPercentageCorrect = keyPercentageCorrect + sk[k];");
           // //tb.WriteLine("\t\t\tend");

           // for (int i = 0; i < InstantiationTemplates.Count; i++)
           //     tb.WriteLine("\t\t\tkeyPercentageCorrect{0} = keyPercentageCorrect{0} / {1};", i + 1, SecretKeys[i]);

           // for (int i = 0; i < InstantiationTemplates.Count; i++)
           //     tb.WriteLine("\t\t\tmismatchPercentage{0} = mismatchPercentage{0} / {1};", i + 1, testOutputVectors.ElementAt(0).Value + 1);

            tb.WriteLine("\t\t\t#1;");
            //tb.WriteLine("\t\t$fwrite(f, \"%b, %f, %f\\n\", inputs, keyPercentageCorrect, mismatchPercentage);");

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\t$fwrite(f{0}, \"%f, %f\\n\", keyPercentageCorrect{0}, mismatchPercentage{0});", i + 1);

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\tmismatchPercentage{0} = 0;", i + 1);

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\tkeyPercentageCorrect{0} = 0;", i + 1);


            //tb.WriteLine("\t\tend");
            tb.WriteLine("\t\tinputs = inputs + {0};", increment_amount);
            //tb.WriteLine("\t\t#1;");
            tb.WriteLine("\tend");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("\t$fclose(f{0});", i + 1);

            tb.WriteLine("\t$finish;");
            tb.WriteLine("end");

            tb.Write("\n");
            tb.WriteLine("endmodule");
            tb.Close();
        }

        internal void GenerateTestBench(string directory)
        {
            string goldenModule = CKT_NAME + "_000pObf_s";
            //string obfuscatedModule = bf.CKT_NAME + "_" + ((int)(bf.obfuscationPercentage * 100)).ToString("000") + "pObf_s";

            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> input_vectors = parseVectors(CKT_INPUTS);
            Dictionary<string, int> output_vectors = parseVectors(CKT_OUTPUTS);
            Dictionary<string, int> testInputVectors = new Dictionary<string, int>();
            Dictionary<string, int> testOutputVectors = new Dictionary<string, int>();

            int inputCount = 0;
            int outputCount = 0;

            foreach (KeyValuePair<string, int> vector in input_vectors)
            {
                if (vector.Value > 1)
                {
                    testInputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    inputCount++;
                }
            }

            if (inputCount > 0) testInputVectors.Add("inputs", inputCount - 1);

            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                if (vector.Value > 1)
                {
                    testOutputVectors.Add(vector.Key, vector.Value);
                }
                else
                {
                    outputCount++;
                }
            }

            if (outputCount > 0) testOutputVectors.Add("outputs", outputCount - 1);


            StreamWriter tb = new StreamWriter(string.Format("{0}FunctionalMismatch{1}.v", directory, CKT_NAME));

            tb.WriteLine("/* Testbench auto-generated by EDA Tool */");
            tb.WriteLine("`timescale 10ns/1ns");
            tb.WriteLine("module GoldenChip_{0} ();\n", CKT_NAME);

            tb.Write("\n");

            tb.WriteLine("/* INPUTS */");

            int max_count = 0;
            int increment_amount = 0;

            foreach (KeyValuePair<string, int> vector in testInputVectors)
            {
                if (vector.Value > 1)
                {
                    var max_value = Math.Pow(2, vector.Value + 1);
                    if (max_value > 65536)
                    {
                        max_count = 65536;
                        increment_amount = (int)((max_value / max_count) - 1);
                    }
                    else
                    {
                        max_count = (int)max_value;
                        increment_amount = 1;
                    }

                    tb.WriteLine("reg [{0}:0] {1} = 0;", vector.Value, vector.Key);
                    tb.WriteLine("localparam MAX_COUNT = {0};", max_count);
                }
                else
                {
                    var max_value = Math.Pow(2, vector.Value + 1);
                    if (max_value > 65536)
                    {
                        max_count = 65536;
                        increment_amount = (int)((max_value / max_count) - 1);
                    }
                    else
                    {
                        max_count = (int)max_value;
                        increment_amount = 1;
                    }

                    tb.WriteLine("reg {0} = 0;", vector.Value);
                    tb.WriteLine("localparam MAX_COUNT = {0};", max_count);
                }
            }

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("reg [{0}:0] sk{1};", SecretKeys[i] - 1, i + 1);
            }

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("localparam [{0}:0] correct_sk{1} = {2}'b{3};", SecretKeys[i] - 1, i + 1, SecretKeys[i], sprng_keys[i]);
            }

            tb.WriteLine("/* OUTPUTS */");
            foreach (KeyValuePair<string, int> vector in testOutputVectors)
            {
                if (vector.Value > 1)
                {
                    tb.WriteLine("wire [{0}:0] {1};", vector.Value, vector.Key);
                    for (int i = 0; i < InstantiationTemplates.Count; i++)
                    {
                        tb.WriteLine("wire [{0}:0] {1}{2};", vector.Value, "obfus" + vector.Key, i + 1);
                    }

                    for (int i = 0; i < InstantiationTemplates.Count; i++)
                    {
                        tb.WriteLine("wire [{0}:0] mismatch{1};", vector.Value, i + 1);
                    }

                }
                // else
                //     tb.WriteLine("write {0};", vector.Value);
            }



            tb.WriteLine("/* END NETS */");
            tb.Write("\n");

            // WRITE INSTANTIATION FOR GOLDEN MODULE ------------------------------------------------------------
            tb.Write("{0} {0} (", goldenModule);

            int inputIndex = 0;
            foreach (KeyValuePair<string, int> vector in input_vectors)
            {
                if (testInputVectors.Contains(vector))
                {
                    tb.Write(".{0}({0}), ", vector.Value);
                }
                else
                {
                    tb.Write(".{0}(inputs[{1}]), ", vector.Key, inputIndex++);
                }
            }

            tb.Write("\n");

            int outputIndex = 0;
            foreach (KeyValuePair<string, int> vector in output_vectors)
            {
                if (testOutputVectors.Contains(vector))
                {
                    if (vector.Key == output_vectors.Last().Key)
                        tb.Write(".{0} (outputs[{1}]));", vector.Key, outputIndex++);
                    else
                        tb.Write(".{0}(outputs[{1}]), ", vector.Key, outputIndex++);
                }
                else
                {
                    if (vector.Key == output_vectors.Last().Key)
                        tb.Write(".{0} (outputs[{1}]));", vector.Key, outputIndex++);
                    else
                        tb.Write(".{0}(outputs[{1}]), ", vector.Key, outputIndex++);
                }
            }

            tb.Write("\n");
            tb.Write("\n");
            // END WRITE INSTANTIATION FOR GOLDEN MODULE ------------------------------------------------------------


            // WRITE INSTANTIATION FOR OBFUSCATED MODULE ------------------------------------------------------------
            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("//" + InstantiationTemplates[i]);
            }
            tb.Write("\n");
            tb.Write("\n");
            // END WRITE INSTANTIATION FOR OBFUSCATED MODULE ------------------------------------------------------------

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                tb.WriteLine("assign mismatch{0} = outputs ^ obfusoutputs{0};", i + 1);
            }

            tb.Write("\n");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("real mismatchPercentage{0} = 0;", i + 1);

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("real keyPercentageCorrect{0} = 0;", i + 1);

            tb.Write("integer ");
            for (int i = 0; i < InstantiationTemplates.Count - 1; i++)
            {
                tb.Write("f{0}, ", i + 1);
            }
            tb.Write("f{0};\n", InstantiationTemplates.Count);

            tb.Write("integer ");
            for (int i = 0; i < InstantiationTemplates.Count - 1; i++)
            {
                tb.Write("rk{0}, ", i + 1);
            }
            tb.Write("rk{0};\n", InstantiationTemplates.Count);

            tb.WriteLine("integer i, j, k, l;");
            tb.WriteLine("initial begin");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("\trk{0} = $fopen(\"RNG1.txt\", \"r\");", i + 1);

            for (int i = 0; i < InstantiationTemplates.Count; i++)
            {
                string[] splitTemplate = InstantiationTemplates[i].Split('_');
                string percentage = splitTemplate[1].Substring(0, 3);
                tb.WriteLine("\tf{0} = $fopen(\"functionalMismatch_{1}_{2}.txt\", \"w\");", i + 1, CKT_NAME, percentage/*((i + 1) * 5).ToString("000")*/);
            }

            // tb.WriteLine("\tf = $fopen(\"functionalMismatch{0}.txt\", \"w\");", obfuscatedModule);
            //     for(int i = 0; i < InstantiationTemplates.Count)
            //tb.WriteLine("$fwrite(f, \"input, sk, mismatch\\n\");");

            tb.WriteLine("end");

            tb.WriteLine("initial begin");
            tb.WriteLine("\tfor(i = 0; i < MAX_COUNT; i = i +1) begin");

            // tb.WriteLine("\t\tfor(l = 0; l < 10; l = l + 1) begin");

            // for (int i = 0; i < InstantiationTemplates.Count; i++)
            //     tb.WriteLine("\t\t\t$fscanf(rk{0}, \"%{1}b\", sk{0});", i + 1, SecretKeys[i]);

            //// tb.WriteLine("\t\t\t#1;");

            // for (int i = 0; i < InstantiationTemplates.Count; i++)
            // {
            //     tb.WriteLine("\t\t\tfor(j = 0; j < {0}; j = j + 1) begin", testOutputVectors.ElementAt(0).Value + 1);
            //     tb.WriteLine("\t\t\t\tmismatchPercentage{0} = mismatchPercentage{0} + mismatch{0}[j];", i + 1);
            //     tb.WriteLine("\t\t\tend");
            // }
            // //tb.WriteLine("\t\t\tfor(j = 0; j < {0}; j = j + 1) begin", testOutputVectors.ElementAt(0).Value + 1);
            // //tb.WriteLine("\t\t\t\tmismatchPercentage = mismatchPercentage + mismatch[j];");
            // //tb.WriteLine("\t\t\tend");

            // for (int i = 0; i < InstantiationTemplates.Count; i++)
            // {
            //     tb.WriteLine("\t\t\tfor(k = 0; k < {0}; k = k + 1) begin", SecretKeys[i]);
            //     tb.WriteLine("\t\t\t\tkeyPercentageCorrect{0} = keyPercentageCorrect{0} + (sk{0}[k] == correct_sk{0}[k]);", i + 1);
            //     tb.WriteLine("\t\t\tend");
            // }
            // //tb.WriteLine("\t\t\tfor(k = 0; k < {0}; k = k + 1) begin", keyBits);
            // //tb.WriteLine("\t\t\t\tkeyPercentageCorrect = keyPercentageCorrect + sk[k];");
            // //tb.WriteLine("\t\t\tend");

            // for (int i = 0; i < InstantiationTemplates.Count; i++)
            //     tb.WriteLine("\t\t\tkeyPercentageCorrect{0} = keyPercentageCorrect{0} / {1};", i + 1, SecretKeys[i]);

            // for (int i = 0; i < InstantiationTemplates.Count; i++)
            //     tb.WriteLine("\t\t\tmismatchPercentage{0} = mismatchPercentage{0} / {1};", i + 1, testOutputVectors.ElementAt(0).Value + 1);

            tb.WriteLine("\t\t\t#1;");
            //tb.WriteLine("\t\t$fwrite(f, \"%b, %f, %f\\n\", inputs, keyPercentageCorrect, mismatchPercentage);");

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\t$fwrite(f{0}, \"%f, %f\\n\", keyPercentageCorrect{0}, mismatchPercentage{0});", i + 1);

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\tmismatchPercentage{0} = 0;", i + 1);

            //for (int i = 0; i < InstantiationTemplates.Count; i++)
            //    tb.WriteLine("\t\t\tkeyPercentageCorrect{0} = 0;", i + 1);


            //tb.WriteLine("\t\tend");
            tb.WriteLine("\t\tinputs = inputs + {0};", increment_amount);
            //tb.WriteLine("\t\t#1;");
            tb.WriteLine("\tend");

            for (int i = 0; i < InstantiationTemplates.Count; i++)
                tb.WriteLine("\t$fclose(f{0});", i + 1);

            tb.WriteLine("\t$finish;");
            tb.WriteLine("end");

            tb.Write("\n");
            tb.WriteLine("endmodule");
            tb.Close();
        }

        internal void WriteBLIF(string directory)
        {
            StreamWriter sw = new StreamWriter(string.Format("{0}{1}_{2}.blif",directory, CKT_NAME, ((int)(100*obfuscationPercentage)).ToString("000")));

            if (this.IsSecure)
                sw.WriteLine("# Locked with Key = {0}", sprng_key);

            sw.WriteLine(".model {0}", CKT_NAME);

            sw.Write(".inputs ");
            foreach (string input in CKT_INPUTS)
            {
                string[] vectors = new string[3];
                if (input.Contains('~'))
                {
                    vectors = input.Split('~');
                    //string inpt = vectors[0] + '[' + vectors[1] + ']';
                    string inpt = vectors[0] + vectors[1];
                    sw.Write("{0} ", inpt);
                }
                else
                {
                    sw.Write("{0} ", input);
                }


            }

            for (int i = 0; i < keyBits; i++)
                sw.Write("keyinput{0} ", i);

            sw.Write("\n");
            sw.Write(".outputs ");

            foreach (string output in CKT_OUTPUTS)
            {
                string[] vectors = new string[3];
                if (output.Contains('~'))
                {
                    vectors = output.Split('~');
                    //string outpt = vectors[0] + '[' + vectors[1] + ']';
                    string outpt = vectors[0] + vectors[1];

                    sw.Write("{0} ", outpt);
                }
                else
                {
                    sw.Write("{0} ", output);
                }


            }

            sw.Write("\n");

            foreach(LUT lut in CKT_LUTS.OrderBy(o => int.Parse(o.LUTID)))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(".names ");

                foreach(string input in lut.LUTinputs)
                {
                    string tmp = input.Replace('[', '~').Replace(']', '~');
                    string[] splt = tmp.Split('~');

                    if(splt.Length > 1)
                    {
                        if (splt[0] == "sk")
                            sb.AppendFormat("keyinput{0} ", splt[1]);
                        else
                        {
                            //string lutinput = splt[0] + "[" + splt[1] + "]";
                            string lutinput = splt[0] + splt[1];

                            sb.AppendFormat(lutinput + " ");
                        }

                    }
                    else
                    {
                        sb.AppendFormat("{0} ", input);
                    }
                }

                string[] lutoutput = lut.LUToutput.Replace('[', '~').Replace(']', '~').Split('~');

                if(lutoutput.Length > 1)
                {
                    sb.AppendFormat("{0}{1}", lutoutput[0], lutoutput[1]);
                }
                else
                {
                    sb.AppendFormat("{0}", lut.LUToutput);
                }
                sw.WriteLine(sb.ToString());
                
                foreach(KeyValuePair<string, string> kvp in lut.TruthTable)
                {
                    sw.WriteLine("{0} {1}", kvp.Key, kvp.Value);
                }

            }

            sw.WriteLine(".end");
            sw.Close();
        }

        internal void WriteBench(string directory)
        {
            StreamWriter sw;
            if (IsSecure)
            {
                sw = new StreamWriter(string.Format("{0}{1}_{2}.bench", directory, CKT_NAME, ((int)(100 * obfuscationPercentage)).ToString("000")));
                sw.WriteLine("# Obfuscated {0} .bench file generated at {1}.", CKT_NAME, System.DateTime.Now);
            }
            else
            {
                sw = new StreamWriter(string.Format("{0}{1}.bench", directory, CKT_NAME));
                sw.WriteLine("# Original {0} .bench file generated at {1}.", CKT_NAME, System.DateTime.Now);
            }

            foreach (string input in CKT_INPUTS)
                sw.WriteLine("INPUT({0})", input);

            if (IsSecure)
            {
                for (int i = 0; i < keyBits; i++)
                    sw.WriteLine("INPUT(keyinput{0})", i);
            }

            foreach (string output in CKT_OUTPUTS)
                sw.WriteLine("OUTPUT({0})", output);

            foreach (LUT lut in CKT_LUTS.OrderBy(o => int.Parse(o.LUTID)))
            {
                var tt = lut.expandTruthTable();
                var tt_hex = lut.BinToHex(tt);
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("{0}\t\t\t = LUT_0x{1} (", lut.LUToutput, tt_hex);

                for (int i = 0; i < lut.LUTinputs.Count; i++)
                {
                    var input = lut.LUTinputs[i].Replace('[', '~').Replace(']', '~');
                    var splt = input.Split('~');

                    if (splt.Length > 1)
                    {
                        if (splt[0] == "sk")
                            sb.AppendFormat("keyinput{0}", splt[1]);
                    }
                    else
                    {
                        sb.AppendFormat("{0}", lut.LUTinputs[i]);
                    }

                    if (i < lut.LUTinputs.Count - 1)
                        sb.AppendFormat(", ");
                }

                sb.AppendFormat(")");
                sw.WriteLine(sb.ToString());
            }

            sw.Close();
        }
    }
}
