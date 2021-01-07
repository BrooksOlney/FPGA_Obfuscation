/* Testbench auto-generated by EDA Tool */
module GoldenChip_ex1010_045pObf_s ();


/* INPUTS */
reg [9:0] inputs = 0;
localparam MAX_COUNT = 1024;
reg [114:0] sk;
/* OUTPUTS */
wire [9:0] outputs;
wire [9:0] obfusoutputs;
wire [9:0] mismatch;
/* END NETS */

ex1010_000pObf_s GoldenModule (.i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), 
.o_0_(outputs[0]), .o_1_(outputs[1]), .o_2_(outputs[2]), .o_3_(outputs[3]), .o_4_(outputs[4]), .o_5_(outputs[5]), .o_6_(outputs[6]), .o_7_(outputs[7]), .o_8_(outputs[8]), .o_9_ (outputs[9]));

ex1010_045pObf_s ObfuscatedModule (.sk(sk), .i_0_(inputs[0]), .i_1_(inputs[1]), .i_2_(inputs[2]), .i_3_(inputs[3]), .i_4_(inputs[4]), .i_5_(inputs[5]), .i_6_(inputs[6]), .i_7_(inputs[7]), .i_8_(inputs[8]), .i_9_(inputs[9]), 
.o_0_(obfusoutputs[0]), .o_1_(obfusoutputs[1]), .o_2_(obfusoutputs[2]), .o_3_(obfusoutputs[3]), .o_4_(obfusoutputs[4]), .o_5_(obfusoutputs[5]), .o_6_(obfusoutputs[6]), .o_7_(obfusoutputs[7]), .o_8_(obfusoutputs[8]), .o_9_ (obfusoutputs[9]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchex1010_045pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%115b", sk);
			#1;
			for(j = 0; j < 10; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 115; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 115;
			mismatchPercentage = mismatchPercentage / 10;
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
