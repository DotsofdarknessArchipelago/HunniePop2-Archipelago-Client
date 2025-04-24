

namespace HunniePop2ArchipelagoClient.Archipelago
{
    public class IDs
    {

        /// <summary>
        /// HELPER METHOD TO CONVERT AN ITEM ID TO A FLAG/ARCHIPELAGO ID SINCE THERE ARE 5 UNIQUE/SHOE GIFT ITEMS
        /// IN THE GAME BUT YOU CAN ONLY GIVE 4 TO A GIRL 
        /// </summary>
        public static int idtoflag(int id)
        {
            if (id == 130) { return 69420093; }
            else if (id == 131) { return 69420094; }
            else if (id == 132) { return 69420095; }
            else if (id == 133) { return 69420096; }
            else if (id == 189) { return 69420141; }
            else if (id == 190) { return 69420142; }
            else if (id == 191) { return 69420143; }
            else if (id == 192) { return 69420144; }

            else if (id == 134) { return 69420097; }
            else if (id == 135) { return 69420098; }
            else if (id == 136) { return 69420099; }
            else if (id == 137) { return 69420100; }
            else if (id == 195) { return 69420145; }
            else if (id == 196) { return 69420146; }
            else if (id == 197) { return 69420147; }
            else if (id == 198) { return 69420148; }

            else if (id == 139) { return 69420101; }
            else if (id == 140) { return 69420102; }
            else if (id == 141) { return 69420103; }
            else if (id == 142) { return 69420104; }
            else if (id == 199) { return 69420149; }
            else if (id == 200) { return 69420150; }
            else if (id == 201) { return 69420151; }
            else if (id == 203) { return 69420152; }

            else if (id == 144) { return 69420105; }
            else if (id == 145) { return 69420106; }
            else if (id == 147) { return 69420107; }
            else if (id == 148) { return 69420108; }
            else if (id == 204) { return 69420153; }
            else if (id == 205) { return 69420154; }
            else if (id == 206) { return 69420155; }
            else if (id == 207) { return 69420156; }

            else if (id == 149) { return 69420109; }
            else if (id == 150) { return 69420110; }
            else if (id == 151) { return 69420111; }
            else if (id == 152) { return 69420112; }
            else if (id == 209) { return 69420157; }
            else if (id == 210) { return 69420158; }
            else if (id == 212) { return 69420159; }
            else if (id == 213) { return 69420160; }

            else if (id == 154) { return 69420113; }
            else if (id == 155) { return 69420114; }
            else if (id == 156) { return 69420115; }
            else if (id == 157) { return 69420116; }
            else if (id == 215) { return 69420161; }
            else if (id == 216) { return 69420162; }
            else if (id == 217) { return 69420163; }
            else if (id == 218) { return 69420164; }

            else if (id == 159) { return 69420117; }
            else if (id == 160) { return 69420118; }
            else if (id == 162) { return 69420119; }
            else if (id == 163) { return 69420120; }
            else if (id == 219) { return 69420165; }
            else if (id == 221) { return 69420166; }
            else if (id == 222) { return 69420167; }
            else if (id == 223) { return 69420168; }

            else if (id == 164) { return 69420121; }
            else if (id == 166) { return 69420122; }
            else if (id == 167) { return 69420123; }
            else if (id == 168) { return 69420124; }
            else if (id == 225) { return 69420169; }
            else if (id == 226) { return 69420170; }
            else if (id == 227) { return 69420171; }
            else if (id == 228) { return 69420172; }

            else if (id == 169) { return 69420125; }
            else if (id == 170) { return 69420126; }
            else if (id == 171) { return 69420127; }
            else if (id == 173) { return 69420128; }
            else if (id == 230) { return 69420173; }
            else if (id == 231) { return 69420174; }
            else if (id == 232) { return 69420175; }
            else if (id == 233) { return 69420176; }

            else if (id == 174) { return 69420129; }
            else if (id == 175) { return 69420130; }
            else if (id == 177) { return 69420131; }
            else if (id == 178) { return 69420132; }
            else if (id == 234) { return 69420177; }
            else if (id == 235) { return 69420178; }
            else if (id == 236) { return 69420179; }
            else if (id == 237) { return 69420180; }

            else if (id == 179) { return 69420133; }
            else if (id == 180) { return 69420134; }
            else if (id == 181) { return 69420135; }
            else if (id == 182) { return 69420136; }
            else if (id == 239) { return 69420181; }
            else if (id == 240) { return 69420182; }
            else if (id == 241) { return 69420183; }
            else if (id == 243) { return 69420184; }

            else if (id == 184) { return 69420137; }
            else if (id == 185) { return 69420138; }
            else if (id == 186) { return 69420139; }
            else if (id == 187) { return 69420140; }
            else if (id == 244) { return 69420185; }
            else if (id == 245) { return 69420186; }
            else if (id == 246) { return 69420187; }
            else if (id == 247) { return 69420188; }
            else { return -1; }
        }

        /// <summary>
        /// HELPER METHOD TO CONVERT A FLAG/ARCHIPELAGO ID TO AN ITEM ID SINCE THERE ARE 5 UNIQUE/SHOE GIFT ITEMS
        /// IN THE GAME BUT YOU CAN ONLY GIVE 4 TO A GIRL 
        /// </summary>
        public static int flagtoid(int flag)
        {
            if (flag == 69420093) { return 130; }
            else if (flag == 69420094) { return 131; }
            else if (flag == 69420095) { return 132; }
            else if (flag == 69420096) { return 133; }
            else if (flag == 69420141) { return 189; }
            else if (flag == 69420142) { return 190; }
            else if (flag == 69420143) { return 191; }
            else if (flag == 69420144) { return 192; }

            else if (flag == 69420097) { return 134; }
            else if (flag == 69420098) { return 135; }
            else if (flag == 69420099) { return 136; }
            else if (flag == 69420100) { return 137; }
            else if (flag == 69420145) { return 195; }
            else if (flag == 69420146) { return 196; }
            else if (flag == 69420147) { return 197; }
            else if (flag == 69420148) { return 198; }

            else if (flag == 69420101) { return 139; }
            else if (flag == 69420102) { return 140; }
            else if (flag == 69420103) { return 141; }
            else if (flag == 69420104) { return 142; }
            else if (flag == 69420149) { return 199; }
            else if (flag == 69420150) { return 200; }
            else if (flag == 69420151) { return 201; }
            else if (flag == 69420152) { return 203; }

            else if (flag == 69420105) { return 144; }
            else if (flag == 69420106) { return 145; }
            else if (flag == 69420107) { return 147; }
            else if (flag == 69420108) { return 148; }
            else if (flag == 69420153) { return 204; }
            else if (flag == 69420154) { return 205; }
            else if (flag == 69420155) { return 206; }
            else if (flag == 69420156) { return 207; }

            else if (flag == 69420109) { return 149; }
            else if (flag == 69420110) { return 150; }
            else if (flag == 69420111) { return 151; }
            else if (flag == 69420112) { return 152; }
            else if (flag == 69420157) { return 209; }
            else if (flag == 69420158) { return 210; }
            else if (flag == 69420159) { return 212; }
            else if (flag == 69420160) { return 213; }

            else if (flag == 69420113) { return 154; }
            else if (flag == 69420114) { return 155; }
            else if (flag == 69420115) { return 156; }
            else if (flag == 69420116) { return 157; }
            else if (flag == 69420161) { return 215; }
            else if (flag == 69420162) { return 216; }
            else if (flag == 69420163) { return 217; }
            else if (flag == 69420164) { return 218; }

            else if (flag == 69420117) { return 159; }
            else if (flag == 69420118) { return 160; }
            else if (flag == 69420119) { return 162; }
            else if (flag == 69420120) { return 163; }
            else if (flag == 69420165) { return 219; }
            else if (flag == 69420166) { return 221; }
            else if (flag == 69420167) { return 222; }
            else if (flag == 69420168) { return 223; }

            else if (flag == 69420121) { return 164; }
            else if (flag == 69420122) { return 166; }
            else if (flag == 69420123) { return 167; }
            else if (flag == 69420124) { return 168; }
            else if (flag == 69420169) { return 225; }
            else if (flag == 69420170) { return 226; }
            else if (flag == 69420171) { return 227; }
            else if (flag == 69420172) { return 228; }

            else if (flag == 69420125) { return 169; }
            else if (flag == 69420126) { return 170; }
            else if (flag == 69420127) { return 171; }
            else if (flag == 69420128) { return 173; }
            else if (flag == 69420173) { return 230; }
            else if (flag == 69420174) { return 231; }
            else if (flag == 69420175) { return 232; }
            else if (flag == 69420176) { return 233; }

            else if (flag == 69420129) { return 174; }
            else if (flag == 69420130) { return 175; }
            else if (flag == 69420131) { return 177; }
            else if (flag == 69420132) { return 178; }
            else if (flag == 69420177) { return 234; }
            else if (flag == 69420178) { return 235; }
            else if (flag == 69420179) { return 236; }
            else if (flag == 69420180) { return 237; }

            else if (flag == 69420133) { return 179; }
            else if (flag == 69420134) { return 180; }
            else if (flag == 69420135) { return 181; }
            else if (flag == 69420136) { return 182; }
            else if (flag == 69420181) { return 239; }
            else if (flag == 69420182) { return 240; }
            else if (flag == 69420183) { return 241; }
            else if (flag == 69420184) { return 243; }

            else if (flag == 69420137) { return 184; }
            else if (flag == 69420138) { return 185; }
            else if (flag == 69420139) { return 186; }
            else if (flag == 69420140) { return 187; }
            else if (flag == 69420185) { return 244; }
            else if (flag == 69420186) { return 245; }
            else if (flag == 69420187) { return 246; }
            else if (flag == 69420188) { return 247; }
            else { return -1; }

        }


        /// <summary>
        /// HELPER METHOD TO CONVERT A ITEM FLAG ID TO AN ITEM ID
        /// </summary>
        public static int itemflagtoid(int flag)
        {
            if (flag == 69420346) { return 250; }
            else if (flag == 69420347) { return 251; }
            else if (flag == 69420348) { return 252; }
            else if (flag == 69420349) { return 253; }
            else if (flag == 69420350) { return 254; }
            else if (flag == 69420351) { return 255; }
            else if (flag == 69420352) { return 256; }
            else if (flag == 69420353) { return 257; }
            else if (flag == 69420354) { return 258; }
            else if (flag == 69420355) { return 259; }
            else if (flag == 69420356) { return 261; }
            else if (flag == 69420357) { return 262; }
            else if (flag == 69420358) { return 263; }
            else if (flag == 69420359) { return 264; }
            else if (flag == 69420360) { return 265; }
            else if (flag == 69420361) { return 266; }
            else if (flag == 69420362) { return 268; }
            else if (flag == 69420363) { return 25; }
            else if (flag == 69420364) { return 26; }
            else if (flag == 69420365) { return 27; }
            else if (flag == 69420366) { return 28; }
            else if (flag == 69420367) { return 29; }
            else if (flag == 69420368) { return 30; }
            else if (flag == 69420369) { return 32; }
            else if (flag == 69420370) { return 31; }
            else if (flag == 69420371) { return 33; }
            else if (flag == 69420372) { return 284; }
            else if (flag == 69420373) { return 285; }
            else if (flag == 69420374) { return 286; }
            else if (flag == 69420375) { return 287; }
            else if (flag == 69420376) { return 288; }
            else if (flag == 69420377) { return 289; }
            else if (flag == 69420378) { return 34; }
            else if (flag == 69420379) { return 35; }
            else if (flag == 69420380) { return 36; }
            else if (flag == 69420381) { return 37; }
            else if (flag == 69420382) { return 38; }
            else if (flag == 69420383) { return 39; }
            else if (flag == 69420384) { return 41; }
            else if (flag == 69420385) { return 40; }
            else if (flag == 69420386) { return 42; }
            else if (flag == 69420387) { return 43; }
            else if (flag == 69420388) { return 44; }
            else if (flag == 69420389) { return 45; }
            else if (flag == 69420390) { return 46; }
            else if (flag == 69420391) { return 47; }
            else if (flag == 69420392) { return 48; }
            else if (flag == 69420393) { return 50; }
            else if (flag == 69420394) { return 49; }
            else if (flag == 69420395) { return 51; }
            else if (flag == 69420396) { return 52; }
            else if (flag == 69420397) { return 249; }
            else if (flag == 69420398) { return 294; }
            else if (flag == 69420399) { return 295; }
            else if (flag == 69420400) { return 296; }
            else if (flag == 69420401) { return 297; }
            else if (flag == 69420402) { return 298; }
            else if (flag == 69420403) { return 299; }
            else if (flag == 69420404) { return 300; }
            else if (flag == 69420405) { return 301; }
            else if (flag == 69420406) { return 269; }
            else if (flag == 69420407) { return 270; }
            else if (flag == 69420408) { return 271; }
            else if (flag == 69420409) { return 272; }
            else if (flag == 69420410) { return 273; }
            else if (flag == 69420411) { return 274; }
            else if (flag == 69420412) { return 275; }
            else if (flag == 69420413) { return 276; }
            else if (flag == 69420414) { return 277; }
            else if (flag == 69420415) { return 278; }
            else if (flag == 69420416) { return 279; }
            else if (flag == 69420417) { return 280; }
            else if (flag == 69420418) { return 281; }
            else if (flag == 69420419) { return 282; }
            else if (flag == 69420420) { return 283; }

            return 0;
        }

        public static int bagflagtoid(int flag)
        {

            return -1;
        }

    }
}

