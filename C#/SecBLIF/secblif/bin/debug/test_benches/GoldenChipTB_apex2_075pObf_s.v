/* Testbench auto-generated by EDA Tool */
module GoldenChip_apex2_075pObf_s ();


/* INPUTS */
reg [38:0] inputs = 0;
localparam MAX_COUNT = 549755813888;
reg [249:0] sk;
/* OUTPUTS */
wire [2:0] outputs;
wire [2:0] obfusoutputs;
wire [2:0] mismatch;
/* END NETS */

apex2_000pObf_s GoldenModule (.i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), .i_10_(inputs[10]), .i_11_(inputs[11]), .i_12_(inputs[12]), .i_13_(inputs[13]), .i_14_(inputs[14]), .i_15_(inputs[15]), .i_16_(inputs[16]), .i_17_(inputs[17]), .i_18_(inputs[18]), .i_19_(inputs[19]), .i_20_(inputs[20]), .i_21_(inputs[21]), .i_22_(inputs[22]), .i_23_(inputs[23]), .i_24_(inputs[24]), .i_25_(inputs[25]), .i_26_(inputs[26]), .i_27_(inputs[27]), .i_28_(inputs[28]), .i_29_(inputs[29]), .i_30_(inputs[30]), .i_31_(inputs[31]), .i_32_(inputs[32]), .i_33_(inputs[33]), .i_34_(inputs[34]), .i_35_(inputs[35]), .i_36_(inputs[36]), .i_37_(inputs[37]), .i_38_(inputs[38]), 
.o_0_(outputs[0]), .o_1_(outputs[1]), .o_2_ (outputs[2]));

apex2_075pObf_s ObfuscatedModule (.sk(sk), .i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), .i_10_(inputs[10]), .i_11_(inputs[11]), .i_12_(inputs[12]), .i_13_(inputs[13]), .i_14_(inputs[14]), .i_15_(inputs[15]), .i_16_(inputs[16]), .i_17_(inputs[17]), .i_18_(inputs[18]), .i_19_(inputs[19]), .i_20_(inputs[20]), .i_21_(inputs[21]), .i_22_(inputs[22]), .i_23_(inputs[23]), .i_24_(inputs[24]), .i_25_(inputs[25]), .i_26_(inputs[26]), .i_27_(inputs[27]), .i_28_(inputs[28]), .i_29_(inputs[29]), .i_30_(inputs[30]), .i_31_(inputs[31]), .i_32_(inputs[32]), .i_33_(inputs[33]), .i_34_(inputs[34]), .i_35_(inputs[35]), .i_36_(inputs[36]), .i_37_(inputs[37]), .i_38_(inputs[38]), 
.o_0_(obfusoutputs[0]), .o_1_(obfusoutputs[1]), .o_2_ (obfusoutputs[2]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchapex2_075pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%250b", sk);
			#1;
			for(j = 0; j < 3; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 250; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 250;
			mismatchPercentage = mismatchPercentage / 3;
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
