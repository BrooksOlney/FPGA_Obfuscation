/* Testbench auto-generated by EDA Tool */
module GoldenChip_misex3_040pObf_s ();


/* INPUTS */
reg [13:0] inputs = 0;
localparam MAX_COUNT = 16384;
reg [105:0] sk;
/* OUTPUTS */
wire [13:0] outputs;
wire [13:0] obfusoutputs;
wire [13:0] mismatch;
/* END NETS */

misex3_000pObf_s GoldenModule (.a(inputs[0]), .b(inputs[1]), .c(inputs[2]), .d(inputs[3]), .e(inputs[4]), .f(inputs[5]), .g(inputs[6]), .h(inputs[7]), .i(inputs[8]), .j(inputs[9]), .k(inputs[10]), .l(inputs[11]), .m(inputs[12]), .n(inputs[13]), 
.r2(outputs[0]), .s2(outputs[1]), .t2(outputs[2]), .u2(outputs[3]), .n2(outputs[4]), .o2(outputs[5]), .p2(outputs[6]), .q2(outputs[7]), .h2(outputs[8]), .i2(outputs[9]), .j2(outputs[10]), .k2(outputs[11]), .m2(outputs[12]), .l2 (outputs[13]));

misex3_040pObf_s ObfuscatedModule (.sk(sk), .a(inputs[0]), .b(inputs[1]), .c(inputs[2]), .d(inputs[3]), .e(inputs[4]), .f(inputs[5]), .g(inputs[6]), .h(inputs[7]), .i(inputs[8]), .j(inputs[9]), .k(inputs[10]), .l(inputs[11]), .m(inputs[12]), .n(inputs[13]), 
.r2(obfusoutputs[0]), .s2(obfusoutputs[1]), .t2(obfusoutputs[2]), .u2(obfusoutputs[3]), .n2(obfusoutputs[4]), .o2(obfusoutputs[5]), .p2(obfusoutputs[6]), .q2(obfusoutputs[7]), .h2(obfusoutputs[8]), .i2(obfusoutputs[9]), .j2(obfusoutputs[10]), .k2(obfusoutputs[11]), .m2(obfusoutputs[12]), .l2 (obfusoutputs[13]));

assign mismatch = outputs ^ obfusoutputs;

real mismatchPercentage = 0;
real keyPercentageCorrect = 0;
integer f, rk, i, j, k, l;
initial begin
	rk = $fopen("RNG1.txt", "r");
	f = $fopen("functionalMismatchmisex3_040pObf_s.txt", "w");
end
initial begin
	for(i = 0; i < MAX_COUNT; i = i +1) begin
		for(l = 0; l < 10; l = l + 1) begin
			$fscanf(rk, "%106b", sk);
			#1;
			for(j = 0; j < 14; j = j + 1) begin
				mismatchPercentage = mismatchPercentage + mismatch[j];
			end
			for(k = 0; k < 106; k = k + 1) begin
				keyPercentageCorrect = keyPercentageCorrect + sk[k];
			end
			keyPercentageCorrect = keyPercentageCorrect / 106;
			mismatchPercentage = mismatchPercentage / 14;
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
