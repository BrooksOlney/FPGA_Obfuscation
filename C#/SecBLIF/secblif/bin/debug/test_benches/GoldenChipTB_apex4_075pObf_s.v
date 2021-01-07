/* Testbench auto-generated by EDA Tool */
module GoldenChip_apex4_075pObf_s ();


/* INPUTS */
reg [8:0] inputs = 0;
localparam MAX_COUNT = 512;
reg [189:0] sk;
/* OUTPUTS */
wire [17:0] outputs;
wire [17:0] obfusoutputs;
wire [17:0] mismatch;
/* END NETS */

apex4_000pObf_s GoldenModule (.i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), 
.o_1_(outputs[0]), .o_2_(outputs[1]), .o_3_(outputs[2]), .o_4_(outputs[3]), .o_5_(outputs[4]), .o_6_(outputs[5]), .o_7_(outputs[6]), .o_8_(outputs[7]), .o_9_(outputs[8]), .o_10_(outputs[9]), .o_11_(outputs[10]), .o_12_(outputs[11]), .o_13_(outputs[12]), .o_14_(outputs[13]), .o_15_(outputs[14]), .o_16_(outputs[15]), .o_17_(outputs[16]), .o_18_ (outputs[17]));

apex4_075pObf_s ObfuscatedModule (.sk(sk), .i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), 
.o_1_(obfusoutputs[0]), .o_2_(obfusoutputs[1]), .o_3_(obfusoutputs[2]), .o_4_(obfusoutputs[3]), .o_5_(obfusoutputs[4]), .o_6_(obfusoutputs[5]), .o_7_(obfusoutputs[6]), .o_8_(obfusoutputs[7]), .o_9_(obfusoutputs[8]), .o_10_(obfusoutputs[9]), .o_11_(obfusoutputs[10]), .o_12_(obfusoutputs[11]), .o_13_(obfusoutputs[12]), .o_14_(obfusoutputs[13]), .o_15_(obfusoutputs[14]), .o_16_(obfusoutputs[15]), .o_17_(obfusoutputs[16]), .o_18_ (obfusoutputs[17]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchapex4_075pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%190b", sk);
			#1;
			for(j = 0; j < 18; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 190; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 190;
			mismatchPercentage = mismatchPercentage / 18;
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
