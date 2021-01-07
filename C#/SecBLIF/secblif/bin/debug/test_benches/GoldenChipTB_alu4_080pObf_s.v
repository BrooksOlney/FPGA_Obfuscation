/* Testbench auto-generated by EDA Tool */
module GoldenChip_alu4_080pObf_s ();


/* INPUTS */
reg [13:0] inputs = 0;
localparam MAX_COUNT = 16384;
reg [237:0] sk;
/* OUTPUTS */
wire [7:0] outputs;
wire [7:0] obfusoutputs;
wire [7:0] mismatch;
/* END NETS */

alu4_000pObf_s GoldenModule (.i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), .i_10_(inputs[10]), .i_11_(inputs[11]), .i_12_(inputs[12]), .i_13_(inputs[13]), 
.o_0_(outputs[0]), .o_1_(outputs[1]), .o_2_(outputs[2]), .o_3_(outputs[3]), .o_4_(outputs[4]), .o_5_(outputs[5]), .o_6_(outputs[6]), .o_7_ (outputs[7]));

alu4_080pObf_s ObfuscatedModule (.sk(sk), .i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), .i_10_(inputs[10]), .i_11_(inputs[11]), .i_12_(inputs[12]), .i_13_(inputs[13]), 
.o_0_(obfusoutputs[0]), .o_1_(obfusoutputs[1]), .o_2_(obfusoutputs[2]), .o_3_(obfusoutputs[3]), .o_4_(obfusoutputs[4]), .o_5_(obfusoutputs[5]), .o_6_(obfusoutputs[6]), .o_7_ (obfusoutputs[7]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchalu4_080pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%238b", sk);
			#1;
			for(j = 0; j < 8; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 238; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 238;
			mismatchPercentage = mismatchPercentage / 8;
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
