using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilador_RCR
{
    public class PilhaChaves
    {
        private List<Int32> adr = new List<Int32>();
        private List<int> nivel = new List<int>();
        private int nivelAtual;

        public void incNivel()
        {
            nivelAtual++;
        }
        public void decNivel()
        {
            nivelAtual--;
        }

        public void add(Int32 i)
        {
            adr.Add(i);
            nivel.Add(nivelAtual);
        }

        public void addElse(Int32 i)
        {
            adr.Insert(adr.Count - 1, i);
            nivel.Add(nivelAtual - 1);
        }

        public bool RemoveNivel()
        {
            if (nivelAtual > 0)
            {
                if (nivel[nivel.Count - 1] == nivelAtual - 1) return true;
                nivelAtual--;
            }
            return false;
        }

        public void remove()
        {
            adr.RemoveAt(adr.Count - 1);
            nivel.RemoveAt(nivel.Count - 1);
        }

        public Int32 getADR()
        {
            return adr[adr.Count - 1];
        }

        public bool fimFuncao()
        {
            if (adr.Count == 1) return true;
            return false;
        }

        public Int32 Length()
        {
            return adr.Count;
        }
    }
}
