namespace SimpleMud
{

// M - String, F string, p int, p row, p col, int abort0ls
    public static class MBScript
    {
        public static void _on_error(IntPtr s, mb_error_e e, string m, string f, int p, ushort row, ushort col, int abort_code) 
        {
            // TODO: Pull UserData, derefence the pointer, and send to logging
            // TODO: Create comm layer to allow TYPEs of message and bitflags to turn off announces
            string type = abort_code == MB_FUNC_WARNING ? "Warning" : "Error";
            if(e == mb_error_e.SE_NO_ERR)
                return;

            if(f is not null) {
                if(e == mb_error_e.SE_RN_REACHED_TO_WRONG_FUNCTION) {
                    Console.WriteLine(
                        $"{type}:\n    Ln {row}, Col {col} in Func: {f}\n    Code {e}, Abort Code {abort_code}\n    Message: {m}.\n");
                } else {
                    var x = e == basic.mb_error_e.SE_EA_EXTENDED_ABORT ? abort_code - basic.MB_EXTENDED_ABORT : abort_code;
                    Console.WriteLine(
                        $"{type}:\n    Ln {row}, Col {col} in File: {f}\n    Code {e}, Abort Code {x}\n    Message: {m}.\n");
                }
            } else {
                var x = e == basic.mb_error_e.SE_EA_EXTENDED_ABORT ? abort_code - basic.MB_EXTENDED_ABORT : abort_code;
                Console.WriteLine($"{type}:\n    Ln {row}, Col {col}\n    Code {e}, Abort Code {abort_code}\n    Message: {m}.\n");
            }
        }

        // TODO: This is not working yet.
        public static int _on_output(IntPtr s, string fmt, ArgIterator arglist) {
            nint pnt = default;
            basic.mb_get_userdata(s, ref pnt);
            var data = Marshal.PtrToStructure<loadedProgram>(pnt);

            string str = __refvalue(arglist.GetNextArg(), string);
            var oot = fmt.Replace("%s", str);
            Console.WriteLine($"{oot}");

            return 1;
        }
    }
}