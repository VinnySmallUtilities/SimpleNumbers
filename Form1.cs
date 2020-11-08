using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleNumbers
{
    public partial class Form1 : Form
    {
        string fileName = "simples.txt";
        string binName  = "simples.bin";
        string binNameD = "simplesDict.bin";
        string fMults   = "nosimples.txt";
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("Kernel32.dll")]
        static extern Int32 CreateFile(string lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, Int32 lpSecurityAttributes,
                                                                Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("Kernel32.dll")]
        static extern Int32 SetFilePointerEx(Int32 hFile, long liDistanceToMove, out long lpNewFilePointer, int dwMoveMethod);

        [DllImport("Kernel32.dll")]
        static unsafe extern Int32 ReadFile(Int32 hFile, byte* buffer, int nNumberOfBytesToRead, out int bytesReaded, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        static unsafe extern Int32 ReadFile(Int32 hFile, byte[] buffer, int nNumberOfBytesToRead, out int bytesReaded, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        static unsafe extern Int32 WriteFile(Int32 hFile, byte[] buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        static unsafe extern Int32 WriteFile(Int32 hFile, byte* buffer, int nNumberOfBytesToWrite, out int NumberOfBytesWritten, int lpOverlapped);

        [DllImport("Kernel32.dll")]
        static extern Int32 CloseHandle(Int32 lpBaseAddress);

        [DllImport("Kernel32.dll")]
        static extern Int32 GetLastError();

        bool terminate = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (terminate == true)
            {
                terminate = false;
                getSimples();
            }
            else
                terminate = true;
        }

        public unsafe static void ULongToBytes(ulong data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[8];

            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 8; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void BytesToULong(out ulong data, byte[] target, long start = 0)
        {
            data = 0;
            if (start < 0 || start + 8 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start + 8 - 1; i >= start; i--)
                {
                    data <<= 8;
                    data += *(t + i);
                }
            }
        }

        /*
        byte[] _n1 = new byte[8], _n2 = new byte[8];
        int tmp; long tmpl;
        private unsafe void getSimples()
        {
            if (!File.Exists(binName) || !File.Exists(binNameD))
            {
                ULongToBytes(3, ref _n1);
                File.WriteAllBytes(binName, _n1);
                ULongToBytes(0, ref _n1);
                File.WriteAllBytes(binNameD, _n1);

                File.WriteAllText(fileName, "2\r\n3\r\n");
                File.WriteAllText(fMults,   "\r\n\r\n3\r\n\r\n");
            }

            int bin = 0, bind = 0;
            try
            {
                ulong currentNumber = 3;
                ulong toAdd = 2;
                bool  isNotFound = false;
                ulong countOfSimples = 1;
                long len;*/ 
                // bin  = CreateFile(binName,  0xC0000000, 0/*0x00000001 | 0x00000002 */ , 0, 3, 0x80 | 0x08000000 /* последовательное сканирование */ /*| 0x20000000*/ /* без буферизации */, 0);
                /*bin  = CreateFile(binName,  0xC0000000, 0, 0, 3, 0x80, 0);
                bind = CreateFile(binNameD, 0xC0000000, 0, 0, 3, 0x80, 0);
                SetFilePointerEx(bin,  0, out len,  2);
                SetFilePointerEx(bin, -8, out tmpl, 2);

                ReadFile(bin, _n1, 8, out tmp, 0);
                BytesToULong(out currentNumber, _n1);

                label2.Text = currentNumber.ToString("0 000");
                label1.Text = countOfSimples.ToString("0 000");
                Application.DoEvents();
                while (!terminate)
                {
                    currentNumber += 2;

                    isNotFound = false;
                    for (long i = 0; i < len; i += 8)
                    {
                        ulong currentSimple, currentMod;
                        getModNumber(i, out currentSimple, out currentMod, bin, bind);
                        
                        currentMod = currentNumber % currentSimple;

                        if (currentMod == 0)
                        {
                            isNotFound = true;
                            #if multi
                                File.AppendAllText(fMults, currentSimple + " ");
                            #else
                                break;
                            #endif
                        }
                    }

                    if (isNotFound)
                    {
                        toAdd += 2;
                        #if multi
                            File.AppendAllText(fMults, "\r\n\r\n");
                        #endif
                        continue;
                    }
                    #if multi
                        File.AppendAllText(fMults, currentNumber + "\r\n\r\n");
                    #endif

                    // нашли простое число
                    countOfSimples++;
                    File.AppendAllText(fileName, currentNumber + "\r\n");

                    for (long i = 0; i < len; i += 8)
                    {
                        ulong currentSimple, currentMod;
                        getModNumber(i, out currentSimple, out currentMod, bin, bind);

                        currentMod += toAdd;
                        if (currentMod >= currentSimple)
                            currentMod = currentMod % currentSimple;

                        // setModNumber(i, currentMod, bind, len);
                    }

                    ULongToBytes(currentNumber, ref _n1);
                    ULongToBytes(0, ref _n2);

                    SetFilePointerEx(bin,  0, out len, 2);
                    SetFilePointerEx(bind, 0, out len, 2);

                    fixed (byte* n1 = _n1, n2 = _n2)
                    {
                        WriteFile(bin,  n1, 8, out tmp, 0);
                        WriteFile(bind, n2, 8, out tmp, 0);
                    }

                    len += 8;

                    toAdd = 2;

                    label2.Text = currentNumber.ToString("000 000 000 000 000 000 000 000");
                    label1.Text = countOfSimples.ToString("000 000 000 000 000 000 000 000");
                    Application.DoEvents();
                }
            }
            finally
            {
                CloseHandle(bin);
                CloseHandle(bind);
            }
        }

        const long constLen = 1024*1024*128;
        ulong[] caches = new ulong[constLen];
        ulong[] cached = new ulong[constLen];
        private unsafe void getModNumber(long i, out ulong currentSimple, out ulong currentMod, int bin, int bind)
        {
            var t = i >> 3;
            if (t < constLen && caches[t] > 0)
            {
                currentSimple = caches[t];
                currentMod    = 0;//cached[t];
            }
            else
            {
                SetFilePointerEx(bin,  i, out tmpl, 0);
                //SetFilePointerEx(bind, i, out tmpl, 0);

                fixed (byte* n1 = _n1)
                {
                    ReadFile(bin,  n1, 8, out tmp, 0);
                }

                BytesToULong(out currentSimple, _n1);
                //BytesToULong(out currentMod,    _n2);
                currentMod    = 0;

                caches[t] = currentSimple;
                //cached[t] = currentMod;
            }
        }

        int lastFlush = 1000;
        private unsafe void setModNumber(long i, ulong currentMod, int bind, long len)
        {
            if ((len >> 3) >= constLen || lastFlush-- < 0)
            {
                lastFlush = 1000;

                SetFilePointerEx(bind, i, out tmpl, 0);

                ULongToBytes(currentMod, ref _n2);

                fixed (byte* n2 = _n2)
                    WriteFile(bind, n2, 8, out tmp, 0);
            }
            cached[i >> 3] = currentMod;
        }
        */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            terminate = true;
        }

        public unsafe static void IntToBytes(int data, ref byte[] target, long start = 0)
        {
            if (target == null)
                target = new byte[4];

            if (start < 0 || start + 4 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start; i < start + 4; i++)
                {
                    *(t + i) = (byte) data;
                    data = data >> 8;
                }
            }
        }

        public unsafe static void BytesToInt(out int data, byte[] target, long start = 0)
        {
            data = 0;
            if (start < 0 || start + 4 > target.LongLength)
                throw new IndexOutOfRangeException();

            fixed (byte * t = target)
            {
                for (long i = start + 4 - 1; i >= start; i--)
                {
                    data <<= 8;
                    data += *(t + i);
                }
            }
        }

        byte[] n1 = new byte[8];
        int tmp;
        byte[] simplies = new byte[1024L*1024L*1024L];
        private unsafe void getSimples()
        {
            File.WriteAllText(fileName, "");
            File.WriteAllText(binName, "");

            label2.Text = "Подготавливается";
            label1.Text = "";
            Application.DoEvents();

            ulong countOfSimples = 1;
            for (long i = 0; i < simplies.LongLength; i++)
                simplies[i] = 0;

            label2.Text = "0";
            label1.Text = "";
            Application.DoEvents();
            if (terminate)
                return;

            int bin  = CreateFile(binName,   0xC0000000, 0, 0, 3, 0x80 | 0x08000000, 0);
            int str  = CreateFile(fileName,  0xC0000000, 0x00000001 | 0x00000002, 0, 3, 0x80 | 0x08000000, 0);

            if (bin <= 0 || str <= 0)
            {
                CloseHandle(str);
                CloseHandle(bin);
                label2.Text = "Ошибка при открытии файла";
                return;
            }

            var nstr = Encoding.GetEncoding(1251).GetBytes("2\r\n");
            WriteFile(str, nstr, nstr.Length, out tmp, 0);

            ULongToBytes((ulong) 2, ref n1);
            WriteFile(bin, n1, 4, out tmp, 0);

            try
            {
                long len = simplies.LongLength/* << 3*/;
                fixed (byte* s = simplies)
                {
                    long a = 3;
                    for (long i = 0; i < len; i++, a += 2)
                    {
                        if (s[i] != 1)
                        {
                            // File.AppendAllText(fileName, "" + a + "\r\n");

                            //ULongToBytes((ulong) a, ref n1);
                            IntToBytes((int) a, ref n1);
                            WriteFile(bin, n1, 4, out tmp, 0);

                            if (tmp != 4)
                                throw new Exception();

                            // if (countOfSimples < 65535)
                            {
                                nstr = Encoding.GetEncoding(1251).GetBytes("" + a + "\r\n");
                                WriteFile(str, nstr, nstr.Length, out tmp, 0);
                            }

                            countOfSimples++;
                            if ((countOfSimples & 1023) == 0)
                            {
                                label2.Text = a.ToString("000 000 000 000 000 000 000 000");
                                label1.Text = countOfSimples.ToString("000 000 000 000 000 000 000 000");
                                Application.DoEvents();
                                if (terminate)
                                    break;
                            }
                        }
                        else
                            continue;

                        for (long j = i + a; j < len; j += a)
                        {
                            // setBit(j, s);
                            s[j] = 1;
                        }
                    }
                }
            }
            finally
            {
                CloseHandle(bin);
                CloseHandle(str);
            }
        }
        /*
        private unsafe void setBit(long j, byte* s)
        {
            int  k = (int) (j & 0x07);
            long l = j >> 3;
            byte c = (byte) (1 << k);

            s[l] = (byte) (s[l] | c);
        }

        private unsafe bool getBit(long j, byte* s)
        {
            int  k = (int) (j & 0x07);
            long l = j >> 3;
            byte c = (byte) (1 << k);

            return (s[l] & c) > 0;
        }
        */

        private unsafe void setBit(long j, byte* s)
        {
            s[j] = 1;
        }

        private unsafe bool getBit(long j, byte* s)
        {
            return s[j] > 0;
        }
    }
}
