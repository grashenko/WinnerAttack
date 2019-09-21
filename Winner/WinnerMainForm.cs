using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using Newtonsoft.Json;
using System.IO;

namespace Wiener
{
    public partial class WienerMainForm : Form
    {
        private Boolean m_bLog = true;

        public WienerMainForm()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
        { 
            BigInteger _bIntPQ = BigInteger.Parse(txtPQ.Text);
            BigInteger _bIntE = BigInteger.Parse(txtE.Text);

            Attack(_bIntE, _bIntPQ);
            
        }

        public void Attack(BigInteger _bIntE, BigInteger _bIntPQ)
        {
            if (cbShowLog.Checked)
                m_bLog = true;
            else
                m_bLog = false;

            UpdateListbox("Calculating secrect key (d, n) of public key (e = " + _bIntE + ", N = " + _bIntPQ + ")");
            ContinuedFractionAttack _contFractAttack = new ContinuedFractionAttack(_bIntE, _bIntPQ);
            BigInteger _bIntD = _contFractAttack.WienerAlgorithm();
            m_bLog = true;
            UpdateListbox("The private key calculated is " + _bIntD);
        }

        public void UpdateListbox(string msg)
        {
            if (m_bLog)
            {
                lstResult.Items.Add(msg);
                lstResult.SelectedIndex = lstResult.Items.Count - 1;
            }
        }

        private List<Key> LoadJson()
        {
            if (openJsonFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(openJsonFileDialog.FileName))
                {
                    string json = sr.ReadToEnd();
                    List<Key> keys = JsonConvert.DeserializeObject<List<Key>>(json);
                    return keys;
                }
            }
            else
                return null;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lstResult.Items.Clear();
        }

        private void btnLoadJson_Click(object sender, EventArgs e)
        {
            List<Key> keys = LoadJson();
            int _iCount = keys.Count - 1;
            while(_iCount >= 0)
            {
                Attack(keys[_iCount].PublicKey, keys[_iCount].Modulus);
                _iCount--;
            }
        }

        private void txtPQ_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
