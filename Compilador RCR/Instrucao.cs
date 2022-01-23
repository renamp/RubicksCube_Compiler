using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilador_RCR
{
    public class Instrucao
    {
        public byte[] instrucao = new byte[16];
        static Int32 index;

        public Int32 length()
        {
            return index;
        }

        public void add(byte dado)
        {
            instrucao[index++] = dado;
        }
        public Instrucao()
        {
            index = 0;
        }
        public Instrucao(byte adr)
        {
            instrucao[0] = adr;
            index = 1;
        }
    }
}
