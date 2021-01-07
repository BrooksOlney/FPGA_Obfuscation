module demo0_o (input reset, input enable, input clk, output [3:0] out);



	wire out[0], out[1], out[2], out[3], g5, g86, g7, g87, g8, g88, g9;
	wire g89, g11, g90, g10, g91, g32, g37, g38, g39, g40, g41;
	wire g42, g44, g45, g46, g47, g48, g49, g50, g51, g52, g53;
	wire g54, g55, g56, g83, g57, g58, g60, g61, g62, g63, g64;
	wire g65, g66, g67, g68, g69, g70, g71, g72, g73, g74, g75;
	wire g76, g77, g78, g79, g80, g81, g82, g84, g85;


	reg g1, g2, g3, g4, g6, g12, g13, g14, g15;
	reg g16, g17, g18, g19, g20, g21, g22, g23, g24;
	reg g25, g26, g27, g28, g29, g30, g31, g33, g34;
	reg g35, g36, g43, g59;

	always @ (posedge g6) begin g1 <= g87; end
	always @ (posedge g6) begin g2 <= g88; end
	always @ (posedge g6) begin g3 <= g89; end
	always @ (posedge g6) begin g4 <= g90; end
	always @ (posedge clk) begin g6 <= g62; end
	always @ (posedge clk) begin g12 <= g63; end
	always @ (posedge clk) begin g13 <= g64; end
	always @ (posedge clk) begin g14 <= g66; end
	always @ (posedge clk) begin g15 <= g67; end
	always @ (posedge clk) begin g16 <= g68; end
	always @ (posedge clk) begin g17 <= g69; end
	always @ (posedge clk) begin g18 <= g70; end
	always @ (posedge clk) begin g19 <= g71; end
	always @ (posedge clk) begin g20 <= g73; end
	always @ (posedge clk) begin g21 <= g74; end
	always @ (posedge clk) begin g22 <= g75; end
	always @ (posedge clk) begin g23 <= g76; end
	always @ (posedge clk) begin g24 <= g56; end
	always @ (posedge clk) begin g25 <= g55; end
	always @ (posedge clk) begin g26 <= g53; end
	always @ (posedge clk) begin g27 <= g54; end
	always @ (posedge clk) begin g28 <= g77; end
	always @ (posedge clk) begin g29 <= g78; end
	always @ (posedge clk) begin g30 <= g79; end
	always @ (posedge clk) begin g31 <= g91; end
	always @ (posedge clk) begin g33 <= g82; end
	always @ (posedge clk) begin g34 <= g50; end
	always @ (posedge clk) begin g35 <= g52; end
	always @ (posedge clk) begin g36 <= g51; end
	always @ (posedge clk) begin g43 <= g80; end
	always @ (posedge clk) begin g59 <= g81; end

	assign out[0] = (((g1)));
	assign out[1] = (((g2)));
	assign out[2] = (((g3)));
	assign out[3] = (((g4)));
	assign g86 = (((!g5)));
	assign g87 = (((!g7) & (!g86) & (g1)) + ((!g7) & (g86) & (g1)) + ((g7) & (g86) & (!g1)) + ((g7) & (g86) & (g1)));
	assign g88 = (((!g7) & (!g8) & (g2)) + ((!g7) & (g8) & (g2)) + ((g7) & (g8) & (!g2)) + ((g7) & (g8) & (g2)));
	assign g89 = (((!g7) & (!g9) & (g3)) + ((!g7) & (g9) & (g3)) + ((g7) & (g9) & (!g3)) + ((g7) & (g9) & (g3)));
	assign g90 = (((!g7) & (!g11) & (g4)) + ((!g7) & (g11) & (g4)) + ((g7) & (g11) & (!g4)) + ((g7) & (g11) & (g4)));
	assign g5 = (((!g1) & (reset)) + ((g1) & (!reset)) + ((g1) & (reset)));
	assign g7 = (((!reset) & (enable)) + ((reset) & (!enable)) + ((reset) & (enable)));
	assign g8 = (((!g1) & (g2) & (!reset)) + ((g1) & (!g2) & (!reset)));
	assign g9 = (((!g3) & (g1) & (g2) & (!reset)) + ((g3) & (!g1) & (!g2) & (!reset)) + ((g3) & (!g1) & (g2) & (!reset)) + ((g3) & (g1) & (!g2) & (!reset)));
	assign g10 = (((!g1) & (!g3)) + ((!g1) & (g3)) + ((g1) & (!g3)));
	assign g11 = (((!reset) & (!g4) & (g2) & (!g10)) + ((!reset) & (g4) & (!g2) & (!g10)) + ((!reset) & (g4) & (!g2) & (g10)) + ((!reset) & (g4) & (g2) & (g10)));
	assign g91 = (((!g31)));
	assign g32 = (((g28) & (g29) & (g30) & (g31)));
	assign g37 = (((g33) & (g34) & (g35) & (g36)));
	assign g38 = (((g26) & (g27) & (g32) & (g37)));
	assign g39 = (((g23) & (g24) & (g25) & (g38)));
	assign g40 = (((g20) & (g21) & (g22) & (g39)));
	assign g41 = (((g17) & (g18) & (g19) & (g40)));
	assign g42 = (((g14) & (g15) & (g16) & (g41)));
	assign g44 = (((g12) & (!g13) & (g42) & (!g43)) + ((g12) & (g13) & (!g42) & (!g43)));
	assign g45 = (((g14) & (g15) & (!g16) & (g41)) + ((g14) & (g15) & (g16) & (!g41)));
	assign g46 = (((!g19) & (!g40) & (g18) & (!g17)) + ((g19) & (g40) & (!g18) & (!g17)));
	assign g47 = (((g20) & (g21) & (!g22) & (g39)) + ((g20) & (g21) & (g22) & (!g39)));
	assign g48 = (((!g23) & (g24) & (g25) & (g38)) + ((g23) & (!g24) & (!g25) & (!g38)) + ((g23) & (!g24) & (!g25) & (g38)) + ((g23) & (!g24) & (g25) & (!g38)) + ((g23) & (!g24) & (g25) & (g38)) + ((g23) & (g24) & (!g25) & (!g38)) + ((g23) & (g24) & (!g25) & (g38)) + ((g23) & (g24) & (g25) & (!g38)));
	assign g49 = (((g34) & (g35) & (g36) & (g32)));
	assign g50 = (((!g34) & (g35) & (g36) & (g32)) + ((g34) & (!g35) & (!g36) & (!g32)) + ((g34) & (!g35) & (!g36) & (g32)) + ((g34) & (!g35) & (g36) & (!g32)) + ((g34) & (!g35) & (g36) & (g32)) + ((g34) & (g35) & (!g36) & (!g32)) + ((g34) & (g35) & (!g36) & (g32)) + ((g34) & (g35) & (g36) & (!g32)));
	assign g51 = (((!g36) & (g32)) + ((g36) & (!g32)));
	assign g52 = (((!g35) & (g36) & (g32)) + ((g35) & (!g36) & (!g32)) + ((g35) & (!g36) & (g32)) + ((g35) & (g36) & (!g32)));
	assign g53 = (((!g26) & (g27) & (g33) & (g49)) + ((g26) & (!g27) & (!g33) & (!g49)) + ((g26) & (!g27) & (!g33) & (g49)) + ((g26) & (!g27) & (g33) & (!g49)) + ((g26) & (!g27) & (g33) & (g49)) + ((g26) & (g27) & (!g33) & (!g49)) + ((g26) & (g27) & (!g33) & (g49)) + ((g26) & (g27) & (g33) & (!g49)));
	assign g54 = (((!g27) & (g33) & (g49)) + ((g27) & (!g33) & (!g49)) + ((g27) & (!g33) & (g49)) + ((g27) & (g33) & (!g49)));
	assign g55 = (((!g25) & (g38)) + ((g25) & (!g38)));
	assign g56 = (((!g24) & (g25) & (g38)) + ((g24) & (!g25) & (!g38)) + ((g24) & (!g25) & (g38)) + ((g24) & (g25) & (!g38)));
	assign g57 = (((g48) & (g83) & (!g55) & (!g56)));
	assign g58 = (((g45) & (g46) & (g47) & (g57)));
	assign g60 = (((g43) & (g12) & (g13) & (g42)));
	assign g61 = (((g44) & (g58) & (!g59) & (!g60)) + ((g44) & (g58) & (g59) & (g60)));
	assign g62 = (((!g6) & (g61)) + ((g6) & (!g61)));
	assign g63 = (((!g12) & (g13) & (g42) & (!g61)) + ((g12) & (!g13) & (!g42) & (!g61)) + ((g12) & (!g13) & (g42) & (!g61)) + ((g12) & (g13) & (!g42) & (!g61)));
	assign g64 = (((!g13) & (g42) & (!g61)) + ((g13) & (!g42) & (!g61)));
	assign g65 = (((!g14) & (g15) & (g16) & (g41)) + ((g14) & (!g15) & (!g16) & (!g41)) + ((g14) & (!g15) & (!g16) & (g41)) + ((g14) & (!g15) & (g16) & (!g41)) + ((g14) & (!g15) & (g16) & (g41)) + ((g14) & (g15) & (!g16) & (!g41)) + ((g14) & (g15) & (!g16) & (g41)) + ((g14) & (g15) & (g16) & (!g41)));
	assign g66 = (((g65) & (!g61)));
	assign g67 = (((!g15) & (g16) & (g41) & (!g61)) + ((g15) & (!g16) & (!g41) & (!g61)) + ((g15) & (!g16) & (g41) & (!g61)) + ((g15) & (g16) & (!g41) & (!g61)));
	assign g68 = (((!g16) & (g41) & (!g61)) + ((g16) & (!g41) & (!g61)));
	assign g69 = (((!g17) & (g18) & (g19) & (g40)) + ((g17) & (!g18) & (!g19) & (!g40)) + ((g17) & (!g18) & (!g19) & (g40)) + ((g17) & (!g18) & (g19) & (!g40)) + ((g17) & (!g18) & (g19) & (g40)) + ((g17) & (g18) & (!g19) & (!g40)) + ((g17) & (g18) & (!g19) & (g40)) + ((g17) & (g18) & (g19) & (!g40)));
	assign g70 = (((!g18) & (g19) & (g40) & (!g61)) + ((g18) & (!g19) & (!g40) & (!g61)) + ((g18) & (!g19) & (g40) & (!g61)) + ((g18) & (g19) & (!g40) & (!g61)));
	assign g71 = (((!g19) & (g40)) + ((g19) & (!g40)));
	assign g72 = (((!g20) & (g21) & (g22) & (g39)) + ((g20) & (!g21) & (!g22) & (!g39)) + ((g20) & (!g21) & (!g22) & (g39)) + ((g20) & (!g21) & (g22) & (!g39)) + ((g20) & (!g21) & (g22) & (g39)) + ((g20) & (g21) & (!g22) & (!g39)) + ((g20) & (g21) & (!g22) & (g39)) + ((g20) & (g21) & (g22) & (!g39)));
	assign g73 = (((g72) & (!g61)));
	assign g74 = (((!g21) & (g22) & (g39) & (!g61)) + ((g21) & (!g22) & (!g39) & (!g61)) + ((g21) & (!g22) & (g39) & (!g61)) + ((g21) & (g22) & (!g39) & (!g61)));
	assign g75 = (((!g22) & (g39) & (!g61)) + ((g22) & (!g39) & (!g61)));
	assign g76 = (((g48) & (!g61)));
	assign g77 = (((!g28) & (g29) & (g30) & (g31)) + ((g28) & (!g29) & (!g30) & (!g31)) + ((g28) & (!g29) & (!g30) & (g31)) + ((g28) & (!g29) & (g30) & (!g31)) + ((g28) & (!g29) & (g30) & (g31)) + ((g28) & (g29) & (!g30) & (!g31)) + ((g28) & (g29) & (!g30) & (g31)) + ((g28) & (g29) & (g30) & (!g31)));
	assign g78 = (((!g29) & (g30) & (g31)) + ((g29) & (!g30) & (!g31)) + ((g29) & (!g30) & (g31)) + ((g29) & (g30) & (!g31)));
	assign g79 = (((!g30) & (g31)) + ((g30) & (!g31)));
	assign g80 = (((!g43) & (g12) & (g13) & (g42)) + ((g43) & (!g12) & (!g13) & (!g42)) + ((g43) & (!g12) & (!g13) & (g42)) + ((g43) & (!g12) & (g13) & (!g42)) + ((g43) & (!g12) & (g13) & (g42)) + ((g43) & (g12) & (!g13) & (!g42)) + ((g43) & (g12) & (!g13) & (g42)) + ((g43) & (g12) & (g13) & (!g42)));
	assign g81 = (((!g59) & (g60)) + ((g59) & (!g60)));
	assign g82 = (((!g33) & (g49) & (!g61)) + ((g33) & (!g49) & (!g61)));
	assign g83 = (((g84) & (g35) & (g34) & (!g33)));
	assign g84 = (((g85) & (g29) & (g28) & (g36)));
	assign g85 = (((!g27) & (!g26) & (g31) & (g30)));

endmodule