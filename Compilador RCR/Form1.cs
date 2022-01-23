using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace Compilador_RCR
{

    public partial class Form1 : Form
    {
        //int[][] m ={{0,0,0,0,0,0,0,0,0},
        //            {0,0,0,0,0,0,0,0,0},
        //            {0,0,0,0,0,0,0,0,0},
        //            {0,0,0,0,0,0,0,0,0},
        //            {0,0,0,0,0,0,0,0,0},
        //            {0,0,0,0,0,0,0,0,0}};

        public void ResetMatrix(){
            // Reset Matrix
            Bitmap image = new Bitmap(pictureBox1.Image);
            Color cor = new Color();

            for( int k=0; k<9; k++ ){
                for( int l=0; l<12; l++){
                    for( int i=0; i<15; i++ ){
                        for( int j=0; j<15; j++ ){
                            if( l==10 && k==4 ) cor = Color.FromArgb(0,255,0);
                            else if(l==7 && k==4 ) cor = Color.FromArgb(255,0,0);
                            else if(l==4 && k==4 ) cor = Color.FromArgb(0,0,255);
                            else if(l==1 && k==4 ) cor = Color.FromArgb(255,128,0);
                            else if(l==7 && k==1 ) cor = Color.FromArgb(255,255,0);
                            else if(l==7 && k==7 ) cor = Color.FromArgb(255,255,255);
                            else cor = Color.FromArgb(120,120,120);

                            image.SetPixel((l*20)+2+i, (k*20)+2+j, cor);
                        }
                    }
                }
            }
            pictureBox1.Image = image;
        }

        public Form1()
        {
            InitializeComponent();
            ResizeTextField();

            openFileDialog1.Filter = "C|*.c|Texto|*.txt|Todos os Arquivos|*.*";
            saveFileDialog1.Filter = "C|*.c|Texto|*.txt|Todos os Arquivos|*.*";

            ResetMatrix();
        }

        private void ResizeTextField()
        {
            try
            {
                Size size = new Size(Size.Width - 40, Size.Height - 93);
                richTextBox1.Size = size;
                Point posicao = new Point(11, Size.Height - 52);
                label2.Location = posicao;
                posicao = new Point(42, Size.Height - 52);
                label3.Location = posicao;
            }
            catch { }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizeTextField();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            StreamWriter file = new StreamWriter( saveFileDialog1.FileName );
            file.Write(richTextBox1.Text);
            file.Close();
        }        

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            StreamReader file = new StreamReader( openFileDialog1.FileName);
            richTextBox1.Text = file.ReadToEnd();
            file.Close();

            saveFileDialog1.InitialDirectory = openFileDialog1.InitialDirectory;
            saveFileDialog1.FileName = openFileDialog1.FileName;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try{
                Font font = new Font(FontFamily.GenericSansSerif, (float)Convert.ToDouble(comboBox1.Text));
                richTextBox1.Font = font;
            }   catch { }
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            try{
                if (e.KeyData == Keys.Enter)
                {
                    Font font = new Font(FontFamily.GenericSansSerif, (float)Convert.ToDouble(comboBox1.Text));
                    richTextBox1.Font = font;
                }
            }   catch { }
        }


        // Abrir
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        // Salvar
        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.FileName.Length > 0)
                saveFileDialog1_FileOk(sender, new CancelEventArgs(false));
            else saveFileDialog1.ShowDialog();
        }

        // Salvar Como
        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        

        // Compilar
        private void button3_Click(object sender, EventArgs e)
        {
            label3.Text = "Compiling.. ";
            if (saveFileDialog1.FileName != "")
            {
                Compiler compile = new Compiler(richTextBox1.Text);
                
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                if (compile.Compile(saveFileDialog1.FileName) == 0)
                {
                    label3.ForeColor = Color.Blue;
                }
                else
                {
                    label3.ForeColor = Color.Red;
                }
                label3.Text = compile.ErroReport;

                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (groupBox1.Visible) groupBox1.Visible = false;
            else groupBox1.Visible = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            int x = -1, y = -1, rotacao = 0, z = 0, nloop = 1;

            if (textBox1.Text.Length > 1 && textBox1.Text[0] == '#') nloop = 4;
            for( int loop = 0; loop < nloop; loop ++){
                rotacao = loop;
            
                int cursor = richTextBox1.SelectionStart;
                string comando = "if( ";

                if (textBox1.Text.Length > 1 && textBox1.Text[0] == '*') rotacao = textBox1.Text[1] - 0x30;
                
                if (sender == buttonElse) comando = "else " + comando;

                for( int k=0; k<9; k++ ){
                    for( int l=0; l<12; l++){
                        if( k > 5 ) x = 5;
                        else if( k > 2 && l > 8 ) x = 0;
                        else if( k > 2 && l > 5 ) x = 1;
                        else if( k > 2 && l > 2 ) x = 2;
                        else if( k > 2 && l >= 0 ) x = 3;
                        else if( k >= 0 ) x = 4;

                        if( k>5 ) y = (l%3) + ((k-8)*(-3));
                        else if( k>2 ) y = (l%3) + ((k-5)*(-3));
                        else y = (l%3) + ((k-2)*(-3));

                        if(!((l==10&&k==4)||(l==7 && k==4 )||(l==4 && k==4 )||(l==1 && k==4 )||(l==7 && k==1 )||(l==7 && k==7 ))){

                            if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((10 * 20) + 3, (4 * 20) + 3))) z = 0;
                            else if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((7 * 20) + 3, (4 * 20) + 3))) z = 1;
                            else if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((4 * 20) + 3, (4 * 20) + 3))) z = 2;
                            else if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((1 * 20) + 3, (4 * 20) + 3))) z = 3;
                            else if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((7 * 20) + 3, (1 * 20) + 3))) z = 4;
                            else if (Color.Equals(image.GetPixel((l * 20) + 3, (k * 20) + 3), image.GetPixel((7 * 20) + 3, (7 * 20) + 3))) z = 5;
                            else x = -1;

                            if (z >= 0 && z <= 3){
                                z += rotacao;
                                if( z > 3 ) z-=4;
                            }
                            if (x >= 0 && x <= 3){
                                x += rotacao;
                                if( x > 3 ) x-=4;
                            }
                            else{
                                for( int i=0; i<rotacao*(((x-3)*2)-1); i++){
                                    if (y == 0) y = 6;
                                    else if (y == 6) y = 8;
                                    else if (y == 8) y = 2;
                                    else if (y == 2) y = 0;

                                    else if (y == 1) y = 3;
                                    else if (y == 3) y = 7;
                                    else if (y == 7) y = 5;
                                    else if (y == 5) y = 1;
                                }
                            }

                            if( x >= 0 )
                                comando += "m[" + x + "][" + y + "] == m[" + z + "][4] && ";
                        }
                    }
                }
            

                comando = comando.Remove(comando.Length - 3) + "){\n";

                if( textBox1.Text.Length > 0 ){
                    comando += "\t";
                    int move=0;
                    for( int i=0; i<textBox1.Text.Length; i++ ){
                        if (textBox1.Text[i] == '*') i += 2;
                        else if (textBox1.Text[i] == '#') i++;

                        move = textBox1.Text[i]-0x30;
                        if (move >= 0 && move <= 3){
                            move += rotacao;
                            if( move > 3 ) move-=4;
                        }

                        if (i + 1 < textBox1.Text.Length && (textBox1.Text[i + 1] == '\'' || textBox1.Text[i + 1] == '-')){
                            comando += "move(8+" + move + ");";
                            i++;
                        }
                        else 
                            comando += "move(" + move + ");";
                    }
                    comando += "\n}\n";
                }
                if (comando.Length == 4) comando = "";
            
                richTextBox1.Text = richTextBox1.Text.Insert(cursor, comando);
                richTextBox1.SelectionStart = cursor + comando.Length;
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            //Point mouse = MousePosition;
            Point mouse = pictureBox1.PointToClient(MousePosition);
            int inix = (((mouse.X / 20)) * 20) + 2;
            int iniy = (((mouse.Y / 20)) * 20) + 2;
            Color cor = new Color();

            Bitmap image = new Bitmap(pictureBox1.Image);

            if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(0, 0, 255)))
                cor = Color.FromArgb(255, 0, 0);
            else if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(255, 0, 0)))
                cor = Color.FromArgb(0, 255, 0);
            else if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(0, 255, 0)))
                cor = Color.FromArgb(255, 128, 0);
            else if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(255, 128, 0)))
                cor = Color.FromArgb(255, 255, 255);
            else if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(255, 255, 255)))
                cor = Color.FromArgb(255, 255, 0);
            else if (Color.Equals(image.GetPixel(inix, iniy), Color.FromArgb(255, 255, 0)))
                cor = Color.FromArgb(120, 120, 120);
            else
                cor = Color.FromArgb(0, 0, 255);

            if (e.Button == MouseButtons.Right) cor = Color.FromArgb(120, 120, 120);

            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 15; j++)
                    image.SetPixel(inix + i, iniy + j, cor);

            pictureBox1.Image = image;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
        }

    }
}
