using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilador_RCR
{
    class Compiler
    {
        private string rawCode;
        private string erroReport;

        public string ErroReport { get { return this.erroReport; } }

        public Compiler(string rawCode)
        {
            this.rawCode = rawCode;
        }

        // remove comentarios
        private string RemoveComent(string rawCode)
        {
            //string semcomentarios = code;
            for (int loop = 0; loop < rawCode.Length; loop++)
            {
                if (rawCode[loop] == '/' && rawCode[loop + 1] == '/')
                {
                    while (rawCode[loop] != '\n')
                    {
                        rawCode = rawCode.Remove(loop, 1);
                    }
                }
            }
            return rawCode;
        }

        private string[] ExtractCode(string rawCode)
        {
            char[] separador = { ' ', '(', ')', ';', '[', ']', '\n', '\t' };
            string[] splitCode;

            rawCode = RemoveComent(rawCode);

            splitCode = rawCode.Split(separador, StringSplitOptions.RemoveEmptyEntries);

            List<string> tempCode = new List<string>();
            for (int i = 0; i < splitCode.Length; i++)
            {
                if (i + 1 < splitCode.Length && (splitCode[i + 1] == "+" || splitCode[i + 1] == "*"))
                {
                    try
                    {
                        if (splitCode[i + 1] == "+")
                        {
                            int soma = Convert.ToByte(splitCode[i]) + Convert.ToByte(splitCode[i + 2]);
                            i += 2;
                            tempCode.Add(soma.ToString());
                        }
                        else if (splitCode[i + 1] == "*")
                        {
                            int mult = Convert.ToByte(splitCode[i]) * Convert.ToByte(splitCode[i + 2]);
                            i += 2;
                            tempCode.Add(mult.ToString());
                        }
                    }
                    catch
                    {
                        tempCode.Add(splitCode[i]);
                    };
                }
                else if (splitCode[i].Length == 3 && (splitCode[i][1] == '+' || splitCode[i][1] == '*'))
                {
                    if (splitCode[i][1] == '+')
                    {
                        int soma = Convert.ToByte(splitCode[i][0] - 0x30) + Convert.ToByte(splitCode[i][2] - 0x30);
                        tempCode.Add(soma.ToString());
                    }
                    else if (splitCode[i][1] == '*')
                    {
                        int mult = Convert.ToByte(splitCode[i][0]) * Convert.ToByte(splitCode[i][2]);
                        tempCode.Add(mult.ToString());
                    }
                }
                else
                {
                    tempCode.Add(splitCode[i]);
                }
            }
            splitCode = tempCode.ToArray();
            return splitCode;
        }

        private int verificaInstrucaoDriver(string Base, string Mov)
        {
            if (Base == "0" || Base == "1" || Base == "2")
            {
                string[] movFormat = { "m0", "m1", "m2", "m3", "m4", "m5", " ", " ", "m0'", "m1'", "m2'", "m3'", "m4'", "m5'", " ", " ", "2m0", "2m1", "2m2", "2m3", "2m4", "2m5" };
                for (int i = 0; i < movFormat.Length; i++)
                {
                    if (string.Compare(Mov, movFormat[i], true) == 0) return i;
                }
            }
            return -1;
        }

        private List<Instrucao> InsertServosInstructions(string fileDrive, List<Instrucao> hexProgram, Int32 AdrInicio)
        {
            Int32 adr = (hexProgram.Count * 16) + AdrInicio;
            Int32 setor = adr / 512;
            int coluna = (adr % 512) / 16;
            hexProgram[1].instrucao[2] = (byte)(setor >> 16);
            hexProgram[1].instrucao[3] = (byte)(setor >> 8);
            hexProgram[1].instrucao[4] = (byte)(setor);
            hexProgram[1].instrucao[1] = (byte)(coluna);


            FileStream Driver = File.Open(fileDrive, FileMode.Open, FileAccess.Read);
            byte[] bufferDriver = new byte[Driver.Length];
            Driver.Read(bufferDriver, 0, (int)Driver.Length);
            string comandosDriver = "";
            foreach (byte temp in bufferDriver)
                comandosDriver += Convert.ToChar(temp);


            // Inicio Codificacao Driver
            char[] separadorDriver = { ' ', '\t', ',', '\n' };
            string[] DriverParam = comandosDriver.Split(separadorDriver, StringSplitOptions.RemoveEmptyEntries);

            int inicioOrientacao = hexProgram.Count;

            for (int j = 0; j < DriverParam.Length; j++)
            {
                // se inicio de Orientacao
                if (DriverParam[j] == "#")
                {
                    //  Configura o Endereco de salto da Orientacao Anterior
                    if (inicioOrientacao < hexProgram.Count)
                    {
                        Int32 adr2 = (hexProgram.Count * 16) + AdrInicio;
                        Int32 setor2 = adr2 / 512;
                        int coluna2 = (adr2 % 512) / 16;
                        hexProgram[inicioOrientacao - 1].instrucao[2] = (byte)(setor2 >> 16);
                        hexProgram[inicioOrientacao - 1].instrucao[3] = (byte)(setor2 >> 8);
                        hexProgram[inicioOrientacao - 1].instrucao[4] = (byte)(setor2);
                        hexProgram[inicioOrientacao - 1].instrucao[1] = (byte)(coluna2);
                    }

                    hexProgram.Add(new Instrucao(0x40));
                    hexProgram[hexProgram.Count - 1].instrucao[5] = byte.Parse(DriverParam[++j]);
                    inicioOrientacao = hexProgram.Count;
                }

                // Instrucoes basicas
                else if (verificaInstrucaoDriver(DriverParam[j], DriverParam[j + 1]) >= 0)
                {
                    hexProgram.Add(new Instrucao(0x41));        // parametro instrucao basica
                    hexProgram[hexProgram.Count - 1].instrucao[1] = byte.Parse(DriverParam[j]);
                    hexProgram[hexProgram.Count - 1].instrucao[2] = (byte)verificaInstrucaoDriver(DriverParam[j], DriverParam[++j]);
                    int posicao = 3;
                    for (int limit = 0; limit < 10; limit++)
                    {
                        try
                        {
                            Convert.ToInt16(DriverParam[j + 1]); break;
                        }
                        catch
                        {
                            if (j + 1 < DriverParam.Length) j++;
                            else break;
                        }

                        if (string.Compare(DriverParam[j], "R0", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 4;
                        else if (string.Compare(DriverParam[j], "R1", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 5;
                        else if (string.Compare(DriverParam[j], "R2", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 6;
                        else if (string.Compare(DriverParam[j], "B0", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 8;
                        else if (string.Compare(DriverParam[j], "B1", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 9;
                        else if (string.Compare(DriverParam[j], "B2", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 10;
                        else if (string.Compare(DriverParam[j], "C", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 17;
                        else if (string.Compare(DriverParam[j], "C2", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 18;
                        else if (string.Compare(DriverParam[j], "C3", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 19;
                    }
                }

                // Instrucao Resumida ( é necessario ter instrucoes basicas sulficiente para essa funcionar )
                else if ((DriverParam[j] == "0" || DriverParam[j] == "1" || DriverParam[j] == "2") && string.Compare(DriverParam[j + 1], "SETUP") == 0)
                {
                    hexProgram.Add(new Instrucao(0x42));                                            // parametro instrucao resumida
                    hexProgram[hexProgram.Count - 1].instrucao[1] = byte.Parse(DriverParam[j++]);         // posicao da base
                    int posicao = 3;
                    j++;
                    // enquanto for um comando movimentacao do cubo
                    for (int limit = 0; limit < 10; limit++)
                    {
                        try
                        {
                            Convert.ToInt16(DriverParam[j]); break;
                        }
                        catch { }

                        if (string.Compare(DriverParam[j], "B0", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 8;
                        else if (string.Compare(DriverParam[j], "B1", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 9;
                        else if (string.Compare(DriverParam[j], "B2", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 10;
                        else if (string.Compare(DriverParam[j], "C", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 17;
                        else if (string.Compare(DriverParam[j], "C2", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 18;
                        else if (string.Compare(DriverParam[j], "C3", true) == 0) hexProgram[hexProgram.Count - 1].instrucao[posicao++] = 19;
                        j++;
                    }

                    hexProgram[hexProgram.Count - 1].instrucao[2] = byte.Parse(DriverParam[j]);
                }
            }
            if (hexProgram[inicioOrientacao - 1].instrucao[0] == 0x40)
            {
                Int32 adr2 = (hexProgram.Count * 16) + AdrInicio;
                Int32 setor2 = adr2 / 512;
                int coluna2 = (adr2 % 512) / 16;
                hexProgram[inicioOrientacao - 1].instrucao[2] = (byte)(setor2 >> 16);
                hexProgram[inicioOrientacao - 1].instrucao[3] = (byte)(setor2 >> 8);
                hexProgram[inicioOrientacao - 1].instrucao[4] = (byte)(setor2);
                hexProgram[inicioOrientacao - 1].instrucao[1] = (byte)(coluna2);
            }
            hexProgram.Add(new Instrucao(0xf8));

            Driver.Close();
            return hexProgram;
        }

        public int Compile(string outFile)
        {   
            string[] code;
            List<Instrucao> hexProgram = new List<Instrucao>();
            Funcoes funcoes = new Funcoes();
            Variaveis var = new Variaveis();
            GoTo jump = new GoTo();
            PilhaChaves chaves = new PilhaChaves();
            string fileDrive="";
            Int32 i, temp1, temp2, AdrInicio;
            
            int erro = 0;
            bool loop1, iniMain;
            AdrInicio = 0;

            code = ExtractCode(this.rawCode);

            // Inicio Gerador de Codigo
            hexProgram.Add(new Instrucao(0x31));            // prepara GoTo main
            hexProgram.Add(new Instrucao(0xF8));            // posicao para o Driver das garras

            iniMain = false;
            for (i = 0; i < code.Length && erro == 0; i++)
            {
                // Instrucao Move
                if (string.Compare(code[i], "move", true) == 0)
                {
                    try
                    {
                        if (hexProgram[hexProgram.Count - 1].instrucao[0] == 0x33 && hexProgram[hexProgram.Count - 1].length() < 16 && string.Compare(code[i - 2], "move", true) == 0)
                        {
                            hexProgram[hexProgram.Count - 1].add(Convert.ToByte(code[++i]));
                        }
                        else
                        {
                            Instrucao tmp = new Instrucao(0x33);
                            tmp.add(Convert.ToByte(code[++i]));
                            hexProgram.Add(tmp);                                        // Adiciona Instrucao
                        }

                        if (hexProgram[hexProgram.Count - 1].length() < 16)
                            hexProgram[hexProgram.Count - 1].instrucao[hexProgram[hexProgram.Count - 1].length()] = 0xFF;
                    }
                    catch { erro = 3; }
                }

                // Adr Inicio Codigo
                else if (string.Compare(code[i], "#inicio", true) == 0)
                {
                    AdrInicio = Convert.ToInt32(code[++i]);
                }

                // Include Driver dos Motores
                else if (string.Compare(code[i], "#include", true) == 0)
                {
                    fileDrive = outFile;

                    for (int j = fileDrive.Length - 1; j >= 0; j--)
                    {
                        if (fileDrive[j] == '\\')
                        {
                            fileDrive = fileDrive.Substring(0, j + 1);
                            break;
                        }
                    }
                    Directory.SetCurrentDirectory(fileDrive);
                    fileDrive += code[i + 1];

                    if (File.Exists(fileDrive))
                    {
                        hexProgram[1].instrucao[0] = 0x31;  // goto driver motor
                    }
                    else erro = 6;
                    i++;
                }

                // identificacao do Fim do Codigo
                else if (string.Compare(code[i], "#fim", true) == 0)
                {
                    hexProgram[hexProgram.Count - 1].instrucao[0] = 0xF8;
                }

                // chamada de goto
                else if (code[i] == "goto" && jump.find(code[i + 1]))
                {

                    Instrucao tmp = new Instrucao(0x31);
                    Int32 adr = jump.getADR(code[++i]);
                    Int32 setor = adr / 512;
                    int coluna = (adr % 512) / 16;
                    tmp.add((byte)(coluna));
                    tmp.add((byte)(setor >> 16));
                    tmp.add((byte)(setor >> 8));
                    tmp.add((byte)(setor));
                    hexProgram.Add(tmp);
                }

                // Instrucao If
                else if (string.Compare(code[i], "if", true) == 0)
                {
                    temp1 = hexProgram.Count;                   // posicao do If
                    hexProgram.Add(new Instrucao(0x21));       // armazena instrucao if

                    loop1 = true;
                    while (loop1 && erro == 0)
                    {
                        i++;
                        // Operacao com Variavel
                        if (var.find(code[i]))
                        {
                            Instrucao newInstrucao = new Instrucao(0x22);
                            newInstrucao.add(0x10);
                            newInstrucao.add((byte)var.getvar(code[i]));

                            // Operador2 é variavel
                            if (var.find(code[i + 2]))
                            {
                                newInstrucao.add(0x83);                             // adiciona comando de variavel
                                newInstrucao.add((byte)var.getvar(code[i + 2]));   // adiciona endereco variavel
                                newInstrucao.add(0);
                            }
                            // Operador2 é Numero
                            else
                            {
                                try
                                {
                                    newInstrucao.add(0x81);                             // adiciona comando de numero
                                    newInstrucao.add(byte.Parse(code[i + 2]));         // adiciona numero
                                    newInstrucao.add(0);                                // segunda parte do numero
                                }
                                catch
                                {
                                    erro = 10;
                                    erroReport = "Erro: '" + code[i + 2] + "' nao encontrado";
                                }
                            }

                            // verifica Operacao
                            if (code[i + 1] == "==") newInstrucao.add(0x81);
                            else if (code[i + 1] == "!=") newInstrucao.add(0x82);
                            else if (code[i + 1] == ">") newInstrucao.add(0x83);
                            else if (code[i + 1] == "<") newInstrucao.add(0x84);
                            else if (code[i + 1] == ">=") newInstrucao.add(0x85);
                            else if (code[i + 1] == "<=") newInstrucao.add(0x86);

                            // Proxima Operacao
                            i += 3;
                            if (code[i] == "{")
                            {
                                newInstrucao.add(0x80);
                                loop1 = false;
                                temp2 = ((hexProgram.Count + 1) * 16) + (AdrInicio);
                                Int32 setor = temp2 / 512;
                                int coluna = (temp2 % 512) / 16;
                                hexProgram[temp1].instrucao[2] = (byte)(setor >> 16);
                                hexProgram[temp1].instrucao[3] = (byte)(setor >> 8);
                                hexProgram[temp1].instrucao[4] = (byte)(setor);
                                hexProgram[temp1].instrucao[1] = (byte)(coluna);

                                chaves.add(temp1);          // posicao do If
                                chaves.incNivel();
                                var.IncNivel();
                            }
                            else if (code[i] == "&&")
                            {
                                newInstrucao.add(0x81);
                            }
                            else if (code[i] == "||")
                            {
                                newInstrucao.add(0x82);
                            }
                            else
                            {
                                erro = 10;
                                erroReport = "Erro: Operacao nao Suportada";
                            }

                            // Adiciona Instrucao a Pilha
                            hexProgram.Add(newInstrucao);
                        }
                        // Operacao com Matrix do Cubo
                        else if (string.Compare(code[i], "m", true) == 0)
                        {
                            int prox = 0;
                            // Operador 1
                            Instrucao tmp = new Instrucao(0x22);
                            tmp.add(Convert.ToByte(code[++i]));
                            tmp.add(Convert.ToByte(code[++i]));

                            // Tipo Operador 2
                            if (string.Compare(code[i + 2], "m", true) == 0)
                            {
                                tmp.add(0x82);
                                tmp.add(Convert.ToByte(code[i + 3]));
                                tmp.add(Convert.ToByte(code[i + 4]));
                                prox = i + 5;
                            }
                            else
                            {
                                try
                                {
                                    if (Convert.ToByte(code[i + 2]) >= 0 && Convert.ToByte(code[i + 2]) <= 255)
                                    {
                                        tmp.add(0x81);
                                        tmp.add(Convert.ToByte(code[i + 2]));
                                        tmp.add(0);
                                        prox = i + 3;
                                    }
                                }
                                catch { }
                            }

                            // Aritmetica
                            if (code[++i] == "==")
                            {
                                tmp.add(0x81);
                            }
                            else if (code[i] == "!=")
                            {
                                tmp.add(0x82);
                            }

                            // Proxima Operacao
                            i = prox;
                            if (code[i] == "{")
                            {
                                tmp.add(0x80);
                                loop1 = false;
                                temp2 = ((hexProgram.Count + 1) * 16) + (AdrInicio);
                                Int32 setor = temp2 / 512;
                                int coluna = (temp2 % 512) / 16;
                                hexProgram[temp1].instrucao[2] = (byte)(setor >> 16);
                                hexProgram[temp1].instrucao[3] = (byte)(setor >> 8);
                                hexProgram[temp1].instrucao[4] = (byte)(setor);
                                hexProgram[temp1].instrucao[1] = (byte)(coluna);

                                chaves.add(temp1);          // posicao do If
                                chaves.incNivel();
                                var.IncNivel();
                            }
                            else if (code[i] == "&&")
                            {
                                tmp.add(0x81);
                            }
                            else if (code[i] == "||")
                            {
                                tmp.add(0x82);
                            }
                            else
                            {
                                erro = 10;
                                erroReport = "Erro: Operacao nao Suportada";
                            }

                            // adiciona Instrucoes na Pilha
                            hexProgram.Add(tmp);
                        }
                        else
                        {
                            erro = 1;
                        }
                    }
                }

                // Identificacao de Fim de instrucao
                else if (code[i] == "}")
                {
                    var.DecNivel(); // libera variaveis

                    if (chaves.Length() > 1)
                    {
                        Int32 aux;
                        bool fimElses = false;

                        if (string.Compare(code[i + 1], "else", true) == 0)
                        {
                            chaves.addElse(hexProgram.Count);

                            if (code[i + 2] == "{")
                            {        // else somente
                                i += 2;
                                //chaves.incNivel();
                                var.IncNivel();
                            }
                            else
                            {
                                i += 1;
                                chaves.decNivel();
                            }

                            hexProgram.Add(new Instrucao(0x31));
                        }
                        else
                            fimElses = true;


                        aux = chaves.getADR();
                        if (hexProgram[aux].instrucao[0] == 0x21)
                        {           // se for um IF
                            Int32 adr = (hexProgram.Count * 16) + AdrInicio;
                            Int32 setor = adr / 512;
                            int coluna = (adr % 512) / 16;
                            hexProgram[aux].instrucao[6] = (byte)(setor >> 16);
                            hexProgram[aux].instrucao[7] = (byte)(setor >> 8);
                            hexProgram[aux].instrucao[8] = (byte)(setor);
                            hexProgram[aux].instrucao[5] = (byte)(coluna);
                            chaves.remove();

                        }
                        else if (hexProgram[aux].instrucao[0] == 0x31)
                        {     // se fim do If true;
                            Int32 adr = (hexProgram.Count * 16) + AdrInicio;
                            Int32 setor = adr / 512;
                            int coluna = (adr % 512) / 16;
                            hexProgram[aux].instrucao[2] = (byte)(setor >> 16);
                            hexProgram[aux].instrucao[3] = (byte)(setor >> 8);
                            hexProgram[aux].instrucao[4] = (byte)(setor);
                            hexProgram[aux].instrucao[1] = (byte)(coluna);
                            chaves.remove();
                        }

                        if (fimElses)
                        {
                            while (chaves.RemoveNivel())
                            {
                                Int32 adr = (hexProgram.Count * 16) + AdrInicio;
                                Int32 setor = adr / 512;
                                int coluna = (adr % 512) / 16;
                                hexProgram[chaves.getADR()].instrucao[2] = (byte)(setor >> 16);
                                hexProgram[chaves.getADR()].instrucao[3] = (byte)(setor >> 8);
                                hexProgram[chaves.getADR()].instrucao[4] = (byte)(setor);
                                hexProgram[chaves.getADR()].instrucao[1] = (byte)(coluna);
                                chaves.remove();
                            }
                        }

                    }
                    else if (chaves.fimFuncao())
                    {
                        // Fim Main
                        if (iniMain)
                        {
                            hexProgram.Add(new Instrucao(0xF8));    // Fim main

                            // configura inicio do Driver motor..
                            if (hexProgram[1].instrucao[0] == 0x31)
                            {
                                hexProgram = InsertServosInstructions(fileDrive, hexProgram, AdrInicio);
                            }
                        }
                        else
                            hexProgram.Add(new Instrucao(0x30));    // Instrucao Return
                        chaves.remove();
                    }
                    else erro = 4;
                }

                // Declaracao de funcao
                else if (string.Compare(code[i], "void", true) == 0)
                {
                    if (string.Compare(code[i + 1], "main", true) == 0)
                    {
                        iniMain = true;                                 // Inicio da Main
                        Int32 adr = (hexProgram.Count * 16) + AdrInicio;
                        Int32 setor = adr / 512;
                        int coluna = (adr % 512) / 16;
                        hexProgram[0].instrucao[2] = (byte)(setor >> 16);
                        hexProgram[0].instrucao[3] = (byte)(setor >> 8);
                        hexProgram[0].instrucao[4] = (byte)(setor);
                        hexProgram[0].instrucao[1] = (byte)(coluna);
                    }

                    if (funcoes.find(code[i + 1]) || var.find(code[i + 1]) || jump.find(code[i + 1]))
                    {
                        erro = 10;
                        erroReport = "Erro: '" + code[i + 1] + "' já existe ";
                    }
                    else
                    {
                        funcoes.add(AdrInicio, code[++i], hexProgram.Count);
                        if (code[++i] == "{")
                        {
                            chaves.add(hexProgram.Count);
                            chaves.incNivel();
                            var.IncNivel();
                        }
                        else erro = 2;
                    }
                }

                // Declaracao de Variavel
                else if (string.Compare(code[i], "int", true) == 0)
                {
                    if (funcoes.find(code[i + 1]) || var.find(code[i + 1]) || jump.find(code[i + 1]))
                    {
                        erro = 10;
                        erroReport = "Erro: '" + code[i + 1] + "' já existe ";
                    }
                    else
                    {
                        var.addVar(code[++i]);
                        Instrucao newInstrucao = new Instrucao(0x26);       // comando de acesso a variavel
                        newInstrucao.add((byte)var.getvar(code[i]));             // posicao da variavel

                        // se inicializacao
                        if (code[i + 1] == "=")
                        {                             // se inicializacao
                                                      // Operador2 é Variavel
                            if (var.find(code[i + 2]))
                            {                     // operador 2 é uma variavel
                                newInstrucao.add(0x82);                     // adiciona instrucao de variavel
                                newInstrucao.add((byte)var.getvar(code[i + 2]));   // Adiciona Operador2
                                newInstrucao.add(0);

                                // se uma operacao com o Operador3
                                if (code[i + 3] == "+" || code[i + 3] == "-" || code[i + 3] == "*" || code[i + 3] == "/")
                                {
                                    int operacao = 0;
                                    if (code[i + 3] == "+") operacao = 1;
                                    else if (code[i + 3] == "-") operacao = 2;
                                    else if (code[i + 3] == "*") operacao = 3;
                                    else if (code[i + 3] == "/") operacao = 4;

                                    // Operador3 é Variavel
                                    if (var.find(code[i + 4]))
                                    {
                                        operacao += 0x80;
                                        newInstrucao.add((byte)operacao);                     // adiciona operacao
                                        newInstrucao.add((byte)var.getvar(code[i + 4]));     // adiciona Operador3
                                    }
                                    // Operador3 é um Numero
                                    else
                                    {
                                        newInstrucao.add((byte)operacao);                     // adiciona operacao
                                        newInstrucao.add(byte.Parse(code[i + 4]));     // adiciona numero
                                    }
                                    i += 4;     // atualiza a posicao
                                }
                                // somente operacao com o Operador2
                                else
                                {
                                    newInstrucao.add(0);            // adicinoa operacao somente com o operador2
                                    i += 2;
                                }
                            }
                            // Operador2 é um Numero
                            else
                            {
                                newInstrucao.add(0x81);
                                newInstrucao.add(byte.Parse(code[i + 2]));
                                i += 2;
                            }
                        }
                        // se somente declaracao da variavel
                        else
                        {
                            newInstrucao.add(0x81);     // Operador 2 é um Numero
                            newInstrucao.add(0x00);     // Operador 2 = 0;
                        }

                        // Adiciona instrucao a Pilha
                        hexProgram.Add(newInstrucao);
                    }
                }

                // Definicao de salto por GoTo
                else if (code[i][code[i].Length - 1] == ':')
                {
                    if (jump.find(code[i]) || funcoes.find(code[i]) || var.find(code[i])) erro = 7;
                    else jump.add(AdrInicio, code[i], hexProgram.Count);
                }

                // Chamada de funcao
                else if (funcoes.find(code[i]))
                {
                    Instrucao tmp = new Instrucao(0x32);
                    Int32 adr = funcoes.getADR(code[i]);
                    Int32 setor = adr / 512;
                    int coluna = (adr % 512) / 16;
                    tmp.add((byte)(coluna));
                    tmp.add((byte)(setor >> 16));
                    tmp.add((byte)(setor >> 8));
                    tmp.add((byte)(setor));
                    hexProgram.Add(tmp);
                }

                // Uso de Variavel
                else if (var.find(code[i]))
                {
                    Instrucao newInstrucao = new Instrucao(0x26);               // comando de acesso a variavel
                    newInstrucao.add((byte)var.getvar(code[i]));               // posicao da variavel

                    // Comando de atribuicao
                    if (code[i + 1] == "=")
                    {
                        // Operador2 é Variavel
                        if (var.find(code[i + 2]))
                        {                             // operador 2 é uma variavel
                            newInstrucao.add(0x82);                             // adiciona instrucao de variavel
                            newInstrucao.add((byte)var.getvar(code[i + 2]));     // Adiciona Operador2
                            newInstrucao.add(0);

                            // Operacao com o Operador3
                            if (code[i + 3] == "+" || code[i + 3] == "-" || code[i + 3] == "*" || code[i + 3] == "/")
                            {
                                int operacao = 0;
                                if (code[i + 3] == "+") operacao = 1;
                                else if (code[i + 3] == "-") operacao = 2;
                                else if (code[i + 3] == "*") operacao = 3;
                                else if (code[i + 3] == "/") operacao = 4;

                                // Operador3 é Variavel
                                if (var.find(code[i + 4]))
                                {
                                    operacao += 0x80;
                                    newInstrucao.add((byte)operacao);                       // adiciona operacao
                                    newInstrucao.add((byte)var.getvar(code[i + 4]));       // adiciona Operador3
                                }
                                // Operador3 é um Numero
                                else
                                {
                                    newInstrucao.add((byte)operacao);                       // adiciona operacao
                                    newInstrucao.add(byte.Parse(code[i + 4]));              // adiciona numero
                                }
                                i += 4;     // atualiza a posicao
                            }
                            // somente operacao com o Operador2
                            else
                            {
                                newInstrucao.add(0);            // adicinoa operacao somente com o operador2
                                i += 2;
                            }
                        }
                        // Operador2 é um Numero
                        else
                        {
                            newInstrucao.add(0x81);
                            newInstrucao.add(byte.Parse(code[i + 2]));
                            i += 2;
                        }

                        // Adiciona instrucao a Pilha
                        hexProgram.Add(newInstrucao);
                    }
                    // Incremento
                    else if (code[i][code[i].Length - 1] == '+' && code[i][code[i].Length - 2] == '+')
                    {
                        newInstrucao.add(0x82);                                     // adiciona instrucao de variavel
                        newInstrucao.add((byte)var.getvar(code[i]));               // Adiciona Operador2
                        newInstrucao.add(0);
                        newInstrucao.add((byte)1);                                // adiciona operacao
                        newInstrucao.add((byte)1);                                // adiciona numero

                        // Adiciona instrucao a Pilha
                        hexProgram.Add(newInstrucao);
                    }
                    // Decremento
                    else if (code[i][code[i].Length - 1] == '-' && code[i][code[i].Length - 2] == '-')
                    {
                        newInstrucao.add(0x82);                                     // adiciona instrucao de variavel
                        newInstrucao.add((byte)var.getvar(code[i]));               // Adiciona Operador2
                        newInstrucao.add(0);
                        newInstrucao.add((byte)2);                                // adiciona operacao
                        newInstrucao.add((byte)1);                                // adiciona numero

                        // Adiciona instrucao a Pilha
                        hexProgram.Add(newInstrucao);
                    }
                    // Instrucao Invalida
                    else
                    {
                        erro = 10;
                        erroReport = "Erro: Comando Invalido";
                    }
                }

                // se Funcao nao declarada
                else
                {
                    erro = 5;
                }

            }

            // Gera HEX do Programa
            if (erro == 0)
            {
                string Fnome = outFile;
                i = Fnome.Length - 1;
                while (Fnome[i] != '.') i--;
                Fnome = Fnome.Remove(i) + ".HEX";
                FileStream file = new FileStream(Fnome, FileMode.Create, FileAccess.Write);

                for (i = 0; i < hexProgram.Count; i++)
                {
                    for (int j = 0; j < 16; j++)
                        file.WriteByte(hexProgram[i].instrucao[j]);
                }
                file.Close();
            }

            if (erro == 0) erroReport = "Compilado";
            else if (erro == 1) erroReport = "Erro : Comando Invalido";
            else if (erro == 2) erroReport = "Erro : '{' Esperada";
            else if (erro == 3) erroReport = "Erro : face para Instrucao Move Esperada";
            else if (erro == 4) erroReport = "Erro : '}' nao esperado";
            else if (erro == 5) erroReport = "Erro : Funcao nao Declarada ou Comando Invalido";
            else if (erro == 6) erroReport = "Erro : Driver nao encontrado ";
            else if (erro == 7) erroReport = "Erro : Redefinicao de GoTo";
            else if (erro != 10) erroReport = "Erro : Error nao defindo!" ;
            return erro;
        }
    }
}
