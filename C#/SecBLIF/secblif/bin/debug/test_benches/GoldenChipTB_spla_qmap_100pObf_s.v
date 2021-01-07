/* Testbench auto-generated by EDA Tool */
module GoldenChip_spla_qmap_100pObf_s ();


/* INPUTS */
reg [15:0] inputs = 0;
localparam MAX_COUNT = 65536;
reg [613:0] sk;
/* OUTPUTS */
wire [45:0] outputs;
wire [45:0] obfusoutputs;
wire [45:0] mismatch;
/* END NETS */

spla_qmap_000pObf_s GoldenModule (.i_0_(inputs[0]), .i_1_(inputs[1]), .i_8_(inputs[2]), .i_6_(inputs[3]), .i_7_(inputs[4]), .i_9_(inputs[5]), .i_10_(inputs[6]), .i_11_(inputs[7]), .i_15_(inputs[8]), .i_12_(inputs[9]), .i_13_(inputs[10]), .i_14_(inputs[11]), .i_4_(inputs[12]), .i_3_(inputs[13]), .i_5_(inputs[14]), .i_2_(inputs[15]), 
.o_0_(outputs[0]), .o_1_(outputs[1]), .o_2_(outputs[2]), .o_3_(outputs[3]), .o_4_(outputs[4]), .o_5_(outputs[5]), .o_6_(outputs[6]), .o_7_(outputs[7]), .o_8_(outputs[8]), .o_9_(outputs[9]), .o_10_(outputs[10]), .o_11_(outputs[11]), .o_12_(outputs[12]), .o_13_(outputs[13]), .o_14_(outputs[14]), .o_15_(outputs[15]), .o_16_(outputs[16]), .o_17_(outputs[17]), .o_18_(outputs[18]), .o_19_(outputs[19]), .o_20_(outputs[20]), .o_21_(outputs[21]), .o_22_(outputs[22]), .o_23_(outputs[23]), .o_24_(outputs[24]), .o_25_(outputs[25]), .o_26_(outputs[26]), .o_27_(outputs[27]), .o_28_(outputs[28]), .o_29_(outputs[29]), .o_30_(outputs[30]), .o_31_(outputs[31]), .o_32_(outputs[32]), .o_33_(outputs[33]), .o_34_(outputs[34]), .o_35_(outputs[35]), .o_36_(outputs[36]), .o_37_(outputs[37]), .o_38_(outputs[38]), .o_39_(outputs[39]), .o_40_(outputs[40]), .o_41_(outputs[41]), .o_42_(outputs[42]), .o_43_(outputs[43]), .o_44_(outputs[44]), .o_45_ (outputs[45]));

spla_qmap_100pObf_s ObfuscatedModule (.sk(sk), .i_0_(inputs[0]), .i_1_(inputs[1]), .i_8_(inputs[2]), .i_6_(inputs[3]), .i_7_(inputs[4]), .i_9_(inputs[5]), .i_10_(inputs[6]), .i_11_(inputs[7]), .i_15_(inputs[8]), .i_12_(inputs[9]), .i_13_(inputs[10]), .i_14_(inputs[11]), .i_4_(inputs[12]), .i_3_(inputs[13]), .i_5_(inputs[14]), .i_2_(inputs[15]), 
.o_0_(obfusoutputs[0]), .o_1_(obfusoutputs[1]), .o_2_(obfusoutputs[2]), .o_3_(obfusoutputs[3]), .o_4_(obfusoutputs[4]), .o_5_(obfusoutputs[5]), .o_6_(obfusoutputs[6]), .o_7_(obfusoutputs[7]), .o_8_(obfusoutputs[8]), .o_9_(obfusoutputs[9]), .o_10_(obfusoutputs[10]), .o_11_(obfusoutputs[11]), .o_12_(obfusoutputs[12]), .o_13_(obfusoutputs[13]), .o_14_(obfusoutputs[14]), .o_15_(obfusoutputs[15]), .o_16_(obfusoutputs[16]), .o_17_(obfusoutputs[17]), .o_18_(obfusoutputs[18]), .o_19_(obfusoutputs[19]), .o_20_(obfusoutputs[20]), .o_21_(obfusoutputs[21]), .o_22_(obfusoutputs[22]), .o_23_(obfusoutputs[23]), .o_24_(obfusoutputs[24]), .o_25_(obfusoutputs[25]), .o_26_(obfusoutputs[26]), .o_27_(obfusoutputs[27]), .o_28_(obfusoutputs[28]), .o_29_(obfusoutputs[29]), .o_30_(obfusoutputs[30]), .o_31_(obfusoutputs[31]), .o_32_(obfusoutputs[32]), .o_33_(obfusoutputs[33]), .o_34_(obfusoutputs[34]), .o_35_(obfusoutputs[35]), .o_36_(obfusoutputs[36]), .o_37_(obfusoutputs[37]), .o_38_(obfusoutputs[38]), .o_39_(obfusoutputs[39]), .o_40_(obfusoutputs[40]), .o_41_(obfusoutputs[41]), .o_42_(obfusoutputs[42]), .o_43_(obfusoutputs[43]), .o_44_(obfusoutputs[44]), .o_45_ (obfusoutputs[45]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchspla_qmap_100pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%614b", sk);
			#1;
			for(j = 0; j < 46; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 614; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 614;
			mismatchPercentage = mismatchPercentage / 46;
			#1;
			$fwrite(f, "%f, %f\n", keyPercentageCorrect, mismatchPercentage);
			mismatchPercentage = 0;
			keyPercentageCorrect = 0;
		end
		inputs = inputs + 1;
		#1;
	end
	$fclose(f);
	$finish;
end

endmodule
