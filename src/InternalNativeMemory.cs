using System;

namespace ArknightsResources.Utility
{
    internal static unsafe class InternalNativeMemory
    {
        public static void* Alloc(int count)
        {
#if NET6_0_OR_GREATER
            return System.Runtime.InteropServices.NativeMemory.AllocZeroed((nuint)count);
#else
            IntPtr intPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(count);
            return intPtr.ToPointer();
#endif
        }

        public static void Free(void* ptr)
        {
#if NET6_0_OR_GREATER
            System.Runtime.InteropServices.NativeMemory.Free(ptr);
#else
            System.Runtime.InteropServices.Marshal.FreeHGlobal(new IntPtr(ptr));
#endif
        }
    }
}
