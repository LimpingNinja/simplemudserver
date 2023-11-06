using System.Runtime.InteropServices;
using bool_t = System.Byte;
using int_t = System.Int32;
using real_t = System.Single;

namespace BittyMud
{
    public static class MBScript
    {
        public class mb_exception : Exception
        {
            public int Code { get; private set; }

            public mb_exception(int code)
            {
                Code = code;
            }
        }

        public class AssertionException : Exception
        {
            public AssertionException(string message) : base(message) { }
        }

        public static void mb_assert(bool condition, string message="Assertion failed.")
        {
            if (!condition)
            {
                throw new AssertionException(message);
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        public const string LIB_NAME = "my_basic";
#elif UNITY_IOS || UNITY_ANDROID
        public const string LIB_NAME = "__Internal";
#elif OS_WINDOWS
        public const string LIB_NAME = "./scripts/my_basic.dll";
#else
        public const string LIB_NAME = "./scripts/my_basic.so";
#endif

        public const bool_t True = (bool_t)1;
        public const bool_t False = (bool_t)0;

        public const int MB_FUNC_OK = 0;
        public const int MB_FUNC_IGNORE = 1;
        public const int MB_FUNC_WARNING = 2;
        public const int MB_FUNC_ERR = 3;
        public const int MB_FUNC_BYE = 4;
        public const int MB_FUNC_SUSPEND = 5;
        public const int MB_FUNC_END = 6;
        public const int MB_LOOP_BREAK = 101;
        public const int MB_LOOP_CONTINUE = 102;
        public const int MB_SUB_RETURN = 103;
        public const int MB_EXTENDED_ABORT = 201;

        public static int mb_check(int hr)
        {
            if (hr != MB_FUNC_OK)
                throw new mb_exception(hr);

            return hr;
        }

        public enum mb_error_e
        {
            SE_NO_ERR = 0,
            /** Common */
            SE_CM_FUNC_EXISTS,
            SE_CM_FUNC_DOES_NOT_EXIST,
            SE_CM_NOT_SUPPORTED,
            /** Parsing */
            SE_PS_FAILED_TO_OPEN_FILE,
            SE_PS_SYMBOL_TOO_LONG,
            SE_PS_INVALID_CHAR,
            SE_PS_INVALID_MODULE,
            SE_PS_DUPLICATE_IMPORT,
            /** Running */
            SE_RN_EMPTY_PROGRAM,
            SE_RN_PROGRAM_TOO_LONG,
            SE_RN_SYNTAX_ERROR,
            SE_RN_OUT_OF_MEMORY,
            SE_RN_OVERFLOW,
            SE_RN_UNEXPECTED_TYPE,
            SE_RN_INVALID_STRING,
            SE_RN_INTEGER_EXPECTED,
            SE_RN_NUMBER_EXPECTED,
            SE_RN_STRING_EXPECTED,
            SE_RN_VAR_EXPECTED,
            SE_RN_INDEX_OUT_OF_BOUND,
            SE_RN_CANNOT_FIND_WITH_THE_SPECIFIC_INDEX,
            SE_RN_TOO_MANY_DIMENSIONS,
            SE_RN_RANK_OUT_OF_BOUND,
            SE_RN_INVALID_ID_USAGE,
            SE_RN_DUPLICATE_ID,
            SE_RN_INCOMPLETE_STRUCTURE,
            SE_RN_LABEL_DOES_NOT_EXIST,
            SE_RN_NO_RETURN_POINT,
            SE_RN_COLON_EXPECTED,
            SE_RN_COMMA_EXPECTED,
            SE_RN_COMMA_OR_SEMICOLON_EXPECTED,
            SE_RN_OPEN_BRACKET_EXPECTED,
            SE_RN_CLOSE_BRACKET_EXPECTED,
            SE_RN_TOO_MANY_NESTED,
            SE_RN_FAILED_TO_OPERATE,
            SE_RN_OPERATOR_EXPECTED,
            SE_RN_ASSIGN_OPERATOR_EXPECTED,
            SE_RN_THEN_EXPECTED,
            SE_RN_ELSE_EXPECTED,
            SE_RN_ENDIF_EXPECTED,
            SE_RN_TO_EXPECTED,
            SE_RN_NEXT_EXPECTED,
            SE_RN_UNTIL_EXPECTED,
            SE_RN_LOOP_VAR_EXPECTED,
            SE_RN_JUMP_LABEL_EXPECTED,
            SE_RN_CALCULATION_ERROR,
            SE_RN_INVALID_EXPRESSION,
            SE_RN_DIVIDE_BY_ZERO,
            SE_RN_REACHED_TO_WRONG_FUNCTION,
            SE_RN_CANNOT_SUSPEND_HERE,
            SE_RN_CANNOT_MIX_INSTRUCTIONAL_AND_STRUCTURED,
            SE_RN_INVALID_ROUTINE,
            SE_RN_ROUTINE_EXPECTED,
            SE_RN_DUPLICATE_ROUTINE,
            SE_RN_INVALID_CLASS,
            SE_RN_CLASS_EXPECTED,
            SE_RN_DUPLICATE_CLASS,
            SE_RN_HASH_AND_COMPARE_MUST_BE_PROVIDED_TOGETHER,
            SE_RN_INVALID_LAMBDA,
            SE_RN_EMPTY_COLLECTION,
            SE_RN_LIST_EXPECTED,
            SE_RN_INVALID_ITERATOR,
            SE_RN_ITERABLE_EXPECTED,
            SE_RN_COLLECTION_EXPECTED,
            SE_RN_COLLECTION_OR_ITERATOR_EXPECTED,
            SE_RN_REFERENCED_TYPE_EXPECTED,
            /** Extended abort */
            SE_EA_EXTENDED_ABORT,
            /** Extra */
            SE_COUNT
        }

        public enum mb_data_e
        {
            MB_DT_NIL = 0,
            MB_DT_UNKNOWN = 1 << 0,
            MB_DT_INT = 1 << 1,
            MB_DT_REAL = 1 << 2,
            MB_DT_NUM = MB_DT_INT | MB_DT_REAL,
            MB_DT_STRING = 1 << 3,
            MB_DT_TYPE = 1 << 4,
            MB_DT_USERTYPE = 1 << 5,
            MB_DT_USERTYPE_REF = 1 << 6,
            MB_DT_ARRAY = 1 << 7,
            MB_DT_LIST = 1 << 8,
            MB_DT_LIST_IT = 1 << 9,
            MB_DT_DICT = 1 << 10,
            MB_DT_DICT_IT = 1 << 11,
            MB_DT_COLLECTION = MB_DT_LIST | MB_DT_DICT,
            MB_DT_ITERATOR = MB_DT_LIST_IT | MB_DT_DICT_IT,
            MB_DT_CLASS = 1 << 12,
            MB_DT_ROUTINE = 1 << 13
        }

        public enum mb_meta_func_e
        {
            MB_MF_IS = 1 << 0,
            MB_MF_ADD = 1 << 1,
            MB_MF_SUB = 1 << 2,
            MB_MF_MUL = 1 << 3,
            MB_MF_DIV = 1 << 4,
            MB_MF_NEG = 1 << 5,
            MB_MF_CALC = MB_MF_IS | MB_MF_ADD | MB_MF_SUB | MB_MF_MUL | MB_MF_DIV | MB_MF_NEG,
            MB_MF_COLL = 1 << 6,
            MB_MF_FUNC = 1 << 7
        }

        public enum mb_meta_status_e
        {
            MB_MS_NONE = 0,
            MB_MS_DONE = 1 << 0,
            MB_MS_RETURNED = 1 << 1
        }

        public enum mb_routine_type_e
        {
            MB_RT_NONE,
            MB_RT_SCRIPT,
            MB_RT_LAMBDA,
            MB_RT_NATIVE
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
        public struct mb_val_bytes_t
        {
            [FieldOffset(0)]
            public IntPtr ptr;
            [FieldOffset(0)]
            public ulong ul;
            [FieldOffset(0)]
            public int_t i;
            [FieldOffset(0)]
            public real_t r;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
        public struct mb_value_u
        {
            [FieldOffset(0)]
            public int_t integer;
            [FieldOffset(0)]
            public real_t float_point;
            [FieldOffset(0)]
            public IntPtr str;
            [FieldOffset(0)]
            public mb_data_e type;
            [FieldOffset(0)]
            public IntPtr usertype;
            [FieldOffset(0)]
            public IntPtr usertype_ref;
            [FieldOffset(0)]
            public IntPtr array;
            [FieldOffset(0)]
            public IntPtr list;
            [FieldOffset(0)]
            public IntPtr list_it;
            [FieldOffset(0)]
            public IntPtr dict;
            [FieldOffset(0)]
            public IntPtr dict_it;
            [FieldOffset(0)]
            public IntPtr instance;
            [FieldOffset(0)]
            public IntPtr routine;
            [FieldOffset(0)]
            public mb_val_bytes_t bytes;

            public string String { get { return Marshal.PtrToStringAnsi(str); } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct mb_value_t
        {
            public mb_data_e type;
            public mb_value_u value;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_func_t(IntPtr s, ref IntPtr l);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_has_routine_arg_func_t(IntPtr s, ref IntPtr l, ref mb_value_t va, uint ca, ref uint ia, IntPtr r);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_pop_routine_arg_func_t(IntPtr s, ref IntPtr l, ref mb_value_t va, uint ca, ref uint ia, IntPtr r, ref mb_value_t val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_routine_func_t(IntPtr s, ref IntPtr l, ref mb_value_t va, uint ca, IntPtr r, [MarshalAs(UnmanagedType.FunctionPtr)]mb_has_routine_arg_func_t has_arg, [MarshalAs(UnmanagedType.FunctionPtr)]mb_pop_routine_arg_func_t pop_arg);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_var_retrieving_func_t(IntPtr s, string name, mb_value_t val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_debug_stepped_handler_t(IntPtr s, ref IntPtr l, string f, int p, ushort row, ushort col);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_error_handler_t(IntPtr s, mb_error_e e, string m, string f, int p, ushort row, ushort col, int abort_code);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_print_func_t(IntPtr s, string fmt, ArgIterator args);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_input_func_t(IntPtr s, string pmt, string buf, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_import_handler_t(IntPtr s, string f);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_dtor_func_t(IntPtr s, IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr mb_clone_func_t(IntPtr s, IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint mb_hash_func_t(IntPtr s, IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_cmp_func_t(IntPtr s, IntPtr lptr, IntPtr rptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_fmt_func_t(IntPtr s, IntPtr ptr, IntPtr buf, uint lb);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_alive_marker_t(IntPtr s, IntPtr h, mb_value_t val);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_alive_checker_t(IntPtr s, IntPtr h, [MarshalAs(UnmanagedType.FunctionPtr)]mb_alive_marker_t marker);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_alive_value_checker_t(IntPtr s, IntPtr h, mb_value_t val, [MarshalAs(UnmanagedType.FunctionPtr)]mb_alive_marker_t marker);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int mb_meta_operator_t(IntPtr s, ref IntPtr l, ref mb_value_t lv, ref mb_value_t rf, ref mb_value_t ret);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate mb_meta_status_e mb_meta_func_t(IntPtr s, ref IntPtr l, ref mb_value_t z, string f);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr mb_memory_allocate_func_t(uint s);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void mb_memory_free_func_t(IntPtr p);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint mb_ver();
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mb_ver_string();

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_init();
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_dispose();
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_open(out IntPtr s);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_close(out IntPtr s);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_reset(ref IntPtr s, bool_t clear_funcs = False, bool_t clear_vars = False);

        [DllImport(MBScript.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_fork(out IntPtr s, IntPtr r, bool_t clear_forked = True);
        [DllImport(MBScript.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_join(out IntPtr s);
        [DllImport(MBScript.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_forked_from(IntPtr s, out IntPtr src);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_register_func(IntPtr s, string n, [MarshalAs(UnmanagedType.FunctionPtr)]mb_func_t f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_remove_func(IntPtr s, string n);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_remove_reserved_func(IntPtr s, string n);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_begin_module(IntPtr s, string n);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_end_module(IntPtr s);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_attempt_func_begin(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_attempt_func_end(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_attempt_open_bracket(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_attempt_close_bracket(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_has_arg(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_pop_int(IntPtr s, ref IntPtr l, out int_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_pop_real(IntPtr s, ref IntPtr l, out real_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_pop_string(IntPtr s, ref IntPtr l, out nint val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_pop_usertype(IntPtr s, ref IntPtr l, out IntPtr val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_pop_value(IntPtr s, ref IntPtr l, out mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_push_int(IntPtr s, ref IntPtr l, int_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_push_real(IntPtr s, ref IntPtr l, real_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_push_string(IntPtr s, ref IntPtr l, string val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_push_usertype(IntPtr s, ref IntPtr l, IntPtr val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_push_value(IntPtr s, ref IntPtr l, mb_value_t val);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_begin_class(IntPtr s, ref IntPtr l, string n, IntPtr meta, int c, out mb_value_t _out);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_end_class(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_class_userdata(IntPtr s, ref IntPtr l, ref IntPtr d);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_class_userdata(IntPtr s, ref IntPtr l, IntPtr d);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_value_by_name(IntPtr s, ref IntPtr l, string n, ref mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_vars(IntPtr s, ref IntPtr l, [MarshalAs(UnmanagedType.FunctionPtr)]mb_var_retrieving_func_t r, int stack_offset = 0);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_add_var(IntPtr s, ref IntPtr l, string n, mb_value_t val, bool_t force);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_var(IntPtr s, ref IntPtr l, ref IntPtr v, bool_t redir);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_var_name(IntPtr s, ref IntPtr v, ref IntPtr n);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_var_value(IntPtr s, IntPtr v, out mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_var_value(IntPtr s, IntPtr v, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_init_array(IntPtr s, ref IntPtr l, mb_data_e t, ref int d, int c, ref IntPtr a);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_array_len(IntPtr s, ref IntPtr l, IntPtr a, int r, ref int i);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_array_elem(IntPtr s, ref IntPtr l, IntPtr a, ref int d, int c, ref mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_array_elem(IntPtr s, ref IntPtr l, IntPtr a, ref int d, int c, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_init_coll(IntPtr s, ref IntPtr l, ref mb_value_t coll);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_coll(IntPtr s, ref IntPtr l, mb_value_t coll, mb_value_t idx, ref mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_coll(IntPtr s, ref IntPtr l, mb_value_t coll, mb_value_t idx, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_remove_coll(IntPtr s, ref IntPtr l, mb_value_t coll, mb_value_t idx);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_count_coll(IntPtr s, ref IntPtr l, mb_value_t coll, ref int c);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_keys_of_coll(IntPtr s, ref IntPtr l, mb_value_t coll, ref mb_value_t keys, int c);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_make_ref_value(IntPtr s, IntPtr val, ref mb_value_t _out, [MarshalAs(UnmanagedType.FunctionPtr)]mb_dtor_func_t un, [MarshalAs(UnmanagedType.FunctionPtr)]mb_clone_func_t cl, [MarshalAs(UnmanagedType.FunctionPtr)]mb_hash_func_t hs/* = null*/, [MarshalAs(UnmanagedType.FunctionPtr)]mb_cmp_func_t cp/* = null*/, [MarshalAs(UnmanagedType.FunctionPtr)]mb_fmt_func_t ft/* = null*/);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_ref_value(IntPtr s, ref IntPtr l, mb_value_t val, ref IntPtr _out);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_ref_value(IntPtr s, ref IntPtr l, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_unref_value(IntPtr s, ref IntPtr l, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_alive_checker(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_alive_checker_t f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_alive_checker_of_value(IntPtr s, ref IntPtr l, mb_value_t val, [MarshalAs(UnmanagedType.FunctionPtr)]mb_alive_value_checker_t f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_override_value(IntPtr s, ref IntPtr l, mb_value_t val, mb_meta_func_e m, [MarshalAs(UnmanagedType.FunctionPtr)]IntPtr f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_dispose_value(IntPtr s, mb_value_t val);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_routine(IntPtr s, ref IntPtr l, string n, ref mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_routine(IntPtr s, ref IntPtr l, string n, [MarshalAs(UnmanagedType.FunctionPtr)]mb_routine_func_t f, bool_t force);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_eval_routine(IntPtr s, ref IntPtr l, mb_value_t val, ref mb_value_t args, uint argc, ref mb_value_t ret);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_routine_type(IntPtr s, mb_value_t val, ref mb_routine_type_e y);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_load_string(IntPtr s, string l, bool_t reset = True);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_load_file(IntPtr s, string f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_run(IntPtr s, bool_t clear_parser = True);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_suspend(IntPtr s, ref IntPtr l);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_schedule_suspend(IntPtr s, int t);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_debug_get(IntPtr s, string n, ref mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_debug_set(IntPtr s, string n, mb_value_t val);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_debug_get_stack_frame_count(IntPtr s);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_debug_get_stack_trace(IntPtr s, string[] fs, uint fc);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_debug_set_stepped_handler(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_debug_stepped_handler_t prev, [MarshalAs(UnmanagedType.FunctionPtr)]mb_debug_stepped_handler_t post);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mb_get_type_string(mb_data_e t);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_raise_error(IntPtr s, ref IntPtr l, mb_error_e err, int ret);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern mb_error_e mb_get_last_error(IntPtr s, out IntPtr file, out int pos, out ushort row, out ushort col);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mb_get_error_desc(mb_error_e err);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_error_handler(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_error_handler_t h);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_printer(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_print_func_t p);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_inputer(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_input_func_t p);

        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_import_handler(IntPtr s, [MarshalAs(UnmanagedType.FunctionPtr)]mb_import_handler_t h);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_memory_manager([MarshalAs(UnmanagedType.FunctionPtr)]mb_memory_allocate_func_t a, [MarshalAs(UnmanagedType.FunctionPtr)]mb_memory_free_func_t f);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool_t mb_get_gc_enabled(IntPtr s);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_gc_enabled(IntPtr s, bool_t gc);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_gc(IntPtr s, ref int_t collected/* = null*/);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_get_userdata(IntPtr s, ref IntPtr d);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_set_userdata(IntPtr s, IntPtr d);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int mb_gets(IntPtr s, IntPtr pmt, IntPtr buf, int n);
        [DllImport(LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr mb_memdup(IntPtr val, uint size);
    }
}