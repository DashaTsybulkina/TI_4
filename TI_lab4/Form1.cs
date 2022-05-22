using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TI_lab4
{
    public partial class Form1 : Form
    {
        string fileName;
        byte[] fileText;
        string fileTextStr;
        string extension;
        string outputStr;


        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            fileName = openFileDialog1.FileName;
            textBoxOutput.Text = "";
            textBoxInput.Text = "";
            textBoxHash.Text = "";
            textBox_r.Text = "";
            textBox_s.Text = "";
            fileText = File.ReadAllBytes(fileName);
            fileTextStr = File.ReadAllText(fileName);
            textBoxInput.Text = fileTextStr;
            extension = Path.GetExtension(fileName);
        }
        private static BigInteger Pow(BigInteger value, BigInteger power, BigInteger mod)
        {
            BigInteger a1 = value;
            BigInteger z1 = power;
            BigInteger x = 1;
            while (z1 != 0)
            {
                while ((z1 % 2) == 0)
                {
                    z1 /= 2;
                    a1 = (a1 * a1) % mod;
                }

                z1--;
                x = (x * a1) % mod;
            }

            return x;
        }

        private BigInteger GetHash(byte[] data, BigInteger q,BigInteger p)
        {
            BigInteger hash = BigInteger.Parse(textBox_h0.Text);
            foreach (var b in data)
            {
                hash = Pow(hash + b, 2, q);
            }
            textBoxHash.Text = hash.ToString();
            return hash;
        }

        public bool MillerRabinTest(BigInteger n, int k)
        {
            if (n == 2 || n == 3)
                return true;
            if (n < 2 || n % 2 == 0)
                return false;
            BigInteger t = n - 1;
            int s = 0;
            while (t % 2 == 0)
            {
                t /= 2;
                s += 1;
            }
            for (int i = 0; i < k; i++)
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] _a = new byte[n.ToByteArray().LongLength];
                BigInteger a;
                do
                {
                    rng.GetBytes(_a);
                    a = new BigInteger(_a);
                }
                while (a < 2 || a >= n - 2);
                BigInteger x = BigInteger.ModPow(a, t, n);
                if (x == 1 || x == n - 1)
                    continue;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == 1)
                        return false;
                    if (x == n - 1)
                        break;
                }
                if (x != n - 1)
                    return false;
            }
            return true;
        }
        private void buttonSign_Click(object sender, EventArgs e)
        {
            if (textBox_q.Text != ""&& textBox_p.Text != ""&& textBox_h.Text != ""&& textBox_x.Text != "" && textBox_k.Text != "") {
                BigInteger q = BigInteger.Parse(textBox_q.Text);
                BigInteger p = BigInteger.Parse(textBox_p.Text);
                BigInteger h = BigInteger.Parse(textBox_h.Text);
                BigInteger x = BigInteger.Parse(textBox_x.Text);
                BigInteger k = BigInteger.Parse(textBox_k.Text);
                BigInteger g = Pow(h, (p - 1) / q, p);
                if (!MillerRabinTest(p, 100) || !MillerRabinTest(q, 100) || ((p - 1) % q != 0) || (h <= 1 || h >= p - 1) || (g <= 1) || (x <= 0 || x >= q) || (k <= 0 && k >= q))
                {
                    MessageBox.Show("Неверные данные!");
                }
                else
                {
                    BigInteger hash = GetHash(fileText, q, p);
                    BigInteger r = Pow(g, k, p) % q;
                    textBox_r.Text = r.ToString();
                    BigInteger s = ((hash + x * r) * Pow(k, q - 2, q)) % q;
                    textBox_s.Text = s.ToString();
                    if (r == 0 || s == 0)
                    {
                        MessageBox.Show("Неверные данные!");
                    }
                    else
                    {
                        textBox_r.Text = r.ToString();
                        textBox_s.Text = s.ToString();
                        outputStr = fileTextStr + ',' + r.ToString() + ',' + s.ToString();
                        textBoxOutput.Text = outputStr;
                        int temp = fileName.IndexOf(extension);
                        string pathout = fileName.Remove(temp, extension.Length);
                        pathout = pathout + "Output" + extension;
                        File.WriteAllText(pathout, outputStr);
                    }
                }
            }
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            if (textBox_q.Text != "" && textBox_p.Text != "" && textBox_h.Text != "" && textBox_x.Text != "" && textBox_k.Text != "")
            {
                BigInteger q = BigInteger.Parse(textBox_q.Text);
                BigInteger p = BigInteger.Parse(textBox_p.Text);
                BigInteger x = BigInteger.Parse(textBox_x.Text);
                BigInteger h = BigInteger.Parse(textBox_h.Text);
                BigInteger g = Pow(h, (p - 1) / q, p);
                if (!MillerRabinTest(p, 100) && !MillerRabinTest(q, 100) && ((p - 1) % q != 0) && (h <= 1 || h >= p - 1) && (g <= 1))
                {
                    MessageBox.Show("Неверные данные!");
                }
                else
                {
                    string[] textStr = fileTextStr.Split(',');
                    fileText = Encoding.UTF8.GetBytes(textStr[0]);
                    BigInteger r = BigInteger.Parse(textStr[1]);
                    BigInteger s = BigInteger.Parse(textStr[2]);
                    BigInteger y = Pow(g, x, p);
                    BigInteger hash = GetHash(fileText, q, p);
                    BigInteger w = Pow(s, q - 2, q);
                    BigInteger u1 = (hash * w) % q;
                    BigInteger u2 = (r * w) % q;
                    textBox3.Text = r.ToString();
                    BigInteger v = ((Pow(g, u1, p) * Pow(y, u2, p)) % p) % q;
                    textBox4.Text = v.ToString();
                    if (v == r)
                    {
                        textBoxOutput.Text = "Подпись подлинна";
                    }
                    else
                    {
                        textBoxOutput.Text = "Подпись некоректна";
                    }
                }
            }
        }
    }
}
