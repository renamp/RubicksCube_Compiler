using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilador_RCR
{
    public class Variaveis
    {
        private List<string> name = new List<string>();
        private List<int> nivel = new List<int>();

        private int NivelChaves;

        public void IncNivel()
        {
            NivelChaves++;
        }

        public void DecNivel()
        {
            // decrementa um nivel
            if (NivelChaves > 0)
                NivelChaves--;

            // remove variaveis dos niveis superiores
            for (int i = 0; i < name.Count; i++)
            {
                if (nivel[i] > NivelChaves)
                {
                    name.RemoveAt(i);
                    nivel.RemoveAt(i--);
                }
            }
        }

        public bool find(string f)
        {
            if (f[f.Length - 1] == ':') f = f.Remove(f.Length - 1);
            else if (f[f.Length - 1] == '+' && f[f.Length - 2] == '+') f = f.Remove(f.Length - 2);
            else if (f[f.Length - 1] == '-' && f[f.Length - 2] == '-') f = f.Remove(f.Length - 2);

            foreach (string i in name)
                if (i == f) return true;
            return false;
        }

        public void addVar(string f)
        {
            name.Add(f);
            nivel.Add(NivelChaves);
        }

        public int getvar(string f)
        {
            if (f[f.Length - 1] == '+' && f[f.Length - 2] == '+') f = f.Remove(f.Length - 2);
            else if (f[f.Length - 1] == '-' && f[f.Length - 2] == '-') f = f.Remove(f.Length - 2);

            for (int i = 0; i < name.Count; i++)
            {
                if (name[i] == f) return i;
            }
            return -1;
        }
    }
}
