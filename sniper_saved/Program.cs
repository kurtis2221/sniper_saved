using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

class Form1 : Form
{
    public const string TITLE = "A mesterlövész v2.33 - Mentés szerkesztő";
    public const string DIR_SAVE = "save";

    const uint ADDR_HPS = 0x27;
    const uint ADDR_STA = 0x4F;
    const uint ADDR_XPK = 0x6B;
    const uint ADDR_MAP = 0x87;
    const uint ADDR_POS = 0x8B;
    const uint ADDR_NAM = 0xAC;

    const int CT_XY = 12;
    const int CT_WD = 64;
    const int CT_HT = 24;
    const int CT_MG = 8;

    const int MAX_WP = 8;
    string[] nm_wp =
    {
        "Glock",
        "S&W",
        "SiG",
        "Ingram",
        "FN Shotgun",
        "M 14",
        "HK G8",
        "P 90"
    };

    int offs;

    ComboBox cb_saves;
    Label lb_saves, lb_name, lb_name2;
    string[] files;

    TextBox tb_x, tb_y, tb_z;
    TextBox tb_hp1, tb_hp2, tb_st1, tb_st2, tb_dx1, tb_dx2, tb_pw;
    TextBox[] tb_wp;
    TextBox tb_kl, tb_xp1, tb_xp2, tb_pt;

    Label lb_x, lb_y, lb_z;
    Label lb_hp, lb_st, lb_dx, lb_pw;
    Label[] lb_wp;
    Label lb_kl, lb_xp, lb_pt;

    Button bt_save, bt_rest, bt_about;

    BinaryReader br;
    BinaryWriter bw;

    public Form1()
    {
        Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        Text = TITLE;
        MaximizeBox = false;
        ClientSize = new Size(350, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition = FormStartPosition.CenterScreen;
        //Mentés választás
        lb_saves = new Label();
        lb_saves.Bounds = new Rectangle(CT_XY, CT_XY, CT_WD, CT_HT);
        lb_saves.Text = "Mentések";
        Controls.Add(lb_saves);
        cb_saves = new ComboBox();
        cb_saves.Bounds = new Rectangle(lb_saves.Right, lb_saves.Top, 256, CT_HT);
        cb_saves.SelectedIndexChanged += cb_saves_SelectedIndexChanged;
        cb_saves.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(cb_saves);
        //Pálya neve
        lb_name = new Label();
        lb_name.Bounds = new Rectangle(CT_XY, lb_saves.Bottom + CT_MG, CT_WD, CT_HT);
        lb_name.Text = "Pálya neve:";
        Controls.Add(lb_name);
        lb_name2 = new Label();
        lb_name2.Bounds = new Rectangle(lb_name.Right, lb_name.Top, 256, CT_HT);
        Controls.Add(lb_name2);
        //X,Y,Z
        lb_x = new Label();
        lb_x.Bounds = new Rectangle(CT_XY, 80, 12, CT_HT);
        lb_x.Text = "X";
        Controls.Add(lb_x);
        tb_x = new TextBox();
        tb_x.Bounds = new Rectangle(lb_x.Right, lb_x.Top, CT_WD, CT_HT);
        Controls.Add(tb_x);
        lb_y = new Label();
        lb_y.Bounds = new Rectangle(tb_x.Right + CT_MG, lb_x.Top, 12, CT_HT);
        lb_y.Text = "Y";
        Controls.Add(lb_y);
        tb_y = new TextBox();
        tb_y.Bounds = new Rectangle(lb_y.Right, lb_x.Top, CT_WD, CT_HT);
        Controls.Add(tb_y);
        lb_z = new Label();
        lb_z.Bounds = new Rectangle(tb_y.Right + CT_MG, lb_x.Top, 12, CT_HT);
        lb_z.Text = "Z";
        Controls.Add(lb_z);
        tb_z = new TextBox();
        tb_z.Bounds = new Rectangle(lb_z.Right, lb_x.Top, CT_WD, CT_HT);
        Controls.Add(tb_z);
        //HP,ST,PW
        lb_hp = new Label();
        lb_hp.Bounds = new Rectangle(CT_XY, lb_x.Bottom + CT_MG, CT_WD, CT_HT);
        lb_hp.Text = "Egészség";
        Controls.Add(lb_hp);
        tb_hp1 = new TextBox();
        tb_hp1.Bounds = new Rectangle(lb_hp.Right, lb_hp.Top, CT_WD, CT_HT);
        Controls.Add(tb_hp1);
        tb_hp2 = new TextBox();
        tb_hp2.Bounds = new Rectangle(tb_hp1.Right + CT_MG, lb_hp.Top, CT_WD, CT_HT);
        Controls.Add(tb_hp2);
        lb_st = new Label();
        lb_st.Bounds = new Rectangle(CT_XY, lb_hp.Bottom + CT_MG, CT_WD, CT_HT);
        lb_st.Text = "Kitartás";
        Controls.Add(lb_st);
        tb_st1 = new TextBox();
        tb_st1.Bounds = new Rectangle(lb_st.Right, lb_st.Top, CT_WD, CT_HT);
        Controls.Add(tb_st1);
        tb_st2 = new TextBox();
        tb_st2.Bounds = new Rectangle(tb_st1.Right + CT_MG, lb_st.Top, CT_WD, CT_HT);
        Controls.Add(tb_st2);
        lb_dx = new Label();
        lb_dx.Bounds = new Rectangle(CT_XY, lb_st.Bottom + CT_MG, CT_WD, CT_HT);
        lb_dx.Text = "Ügyesség";
        Controls.Add(lb_dx);
        tb_dx1 = new TextBox();
        tb_dx1.Bounds = new Rectangle(lb_dx.Right, lb_dx.Top, CT_WD, CT_HT);
        Controls.Add(tb_dx1);
        tb_dx2 = new TextBox();
        tb_dx2.Bounds = new Rectangle(tb_dx1.Right + CT_MG, lb_dx.Top, CT_WD, CT_HT);
        Controls.Add(tb_dx2);
        lb_pw = new Label();
        lb_pw.Bounds = new Rectangle(CT_XY, lb_dx.Bottom + CT_MG, CT_WD, CT_HT);
        lb_pw.Text = "Erő";
        Controls.Add(lb_pw);
        tb_pw = new TextBox();
        tb_pw.Bounds = new Rectangle(lb_pw.Right, lb_pw.Top, CT_WD, CT_HT);
        Controls.Add(tb_pw);
        //Kill,XP,PT
        lb_kl = new Label();
        lb_kl.Bounds = new Rectangle(CT_XY, lb_pw.Bottom + CT_MG, CT_WD, CT_HT);
        lb_kl.Text = "Ölések";
        Controls.Add(lb_kl);
        tb_kl = new TextBox();
        tb_kl.Bounds = new Rectangle(lb_kl.Right, lb_kl.Top, CT_WD, CT_HT);
        Controls.Add(tb_kl);
        lb_xp = new Label();
        lb_xp.Bounds = new Rectangle(CT_XY, lb_kl.Bottom + CT_MG, CT_WD, CT_HT);
        lb_xp.Text = "XP";
        Controls.Add(lb_xp);
        tb_xp1 = new TextBox();
        tb_xp1.Bounds = new Rectangle(lb_xp.Right, lb_xp.Top, CT_WD, CT_HT);
        Controls.Add(tb_xp1);
        tb_xp2 = new TextBox();
        tb_xp2.Bounds = new Rectangle(tb_xp1.Right + CT_MG, lb_xp.Top, CT_WD, CT_HT);
        Controls.Add(tb_xp2);
        lb_pt = new Label();
        lb_pt.Bounds = new Rectangle(CT_XY, lb_xp.Bottom + CT_MG, CT_WD, CT_HT);
        lb_pt.Text = "Pontok";
        Controls.Add(lb_pt);
        tb_pt = new TextBox();
        tb_pt.Bounds = new Rectangle(lb_pt.Right, lb_pt.Top, CT_WD, CT_HT);
        Controls.Add(tb_pt);
        //Fegyverek
        lb_wp = new Label[MAX_WP];
        tb_wp = new TextBox[MAX_WP];
        for (int i = 0, y = lb_pt.Bottom + CT_MG; i < MAX_WP - 1; i += 2, y += CT_HT + CT_MG)
        {
            lb_wp[i] = new Label();
            lb_wp[i].Bounds = new Rectangle(CT_XY, y, CT_WD, CT_HT);
            lb_wp[i].Text = nm_wp[i];
            Controls.Add(lb_wp[i]);
            tb_wp[i] = new TextBox();
            tb_wp[i].Bounds = new Rectangle(lb_wp[i].Right, y, CT_WD, CT_HT);
            Controls.Add(tb_wp[i]);
            lb_wp[i + 1] = new Label();
            lb_wp[i + 1].Bounds = new Rectangle(tb_wp[i].Right + CT_MG, y, CT_WD, CT_HT);
            lb_wp[i + 1].Text = nm_wp[i + 1];
            Controls.Add(lb_wp[i + 1]);
            tb_wp[i + 1] = new TextBox();
            tb_wp[i + 1].Bounds = new Rectangle(lb_wp[i + 1].Right, y, CT_WD, CT_HT);
            Controls.Add(tb_wp[i + 1]);
        }
        //Gombok
        bt_save = new Button();
        bt_save.Bounds = new Rectangle(CT_XY, lb_wp[MAX_WP - 1].Bottom + CT_MG, 104, CT_HT);
        bt_save.Text = "Mentés";
        bt_save.Click += bt_save_Click;
        Controls.Add(bt_save);
        bt_rest = new Button();
        bt_rest.Bounds = new Rectangle(bt_save.Right + CT_MG, bt_save.Top, 104, CT_HT);
        bt_rest.Text = "Helyreállít";
        bt_rest.Click += bt_rest_Click;
        bt_rest.Enabled = false;
        Controls.Add(bt_rest);
        bt_about = new Button();
        bt_about.Bounds = new Rectangle(bt_rest.Right + CT_MG, bt_save.Top, 104, CT_HT);
        bt_about.Text = "Programról";
        bt_about.Click += bt_about_Click;
        Controls.Add(bt_about);
        files = Directory.GetFiles(DIR_SAVE, "*.sav");
        foreach (string f in files)
            cb_saves.Items.Add(f + " - " + File.GetLastWriteTime(f).ToString());
    }

    void cb_saves_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            int tmp;
            br = new BinaryReader(new FileStream(files[cb_saves.SelectedIndex], FileMode.Open, FileAccess.Read), Encoding.Default);
            br.BaseStream.Position = ADDR_HPS;
            tb_hp1.Text = br.ReadSingle().ToString();
            tb_hp2.Text = br.ReadSingle().ToString();
            tb_st1.Text = br.ReadSingle().ToString();
            tb_st2.Text = br.ReadSingle().ToString();
            tb_dx1.Text = br.ReadSingle().ToString();
            tb_dx2.Text = br.ReadSingle().ToString();
            tb_pw.Text = br.ReadSingle().ToString();
            br.BaseStream.Position = ADDR_STA;
            for (int i = 0; i < MAX_WP; i++)
                tb_wp[i].Text = br.ReadSingle().ToString();
            br.BaseStream.Position = ADDR_XPK;
            tb_kl.Text = br.ReadSingle().ToString();
            tb_xp1.Text = br.ReadSingle().ToString();
            tb_xp2.Text = br.ReadInt32().ToString();
            tb_pt.Text = br.ReadInt32().ToString();
            br.BaseStream.Position = ADDR_MAP;
            offs = br.ReadInt32();
            br.BaseStream.Position = ADDR_POS + offs;
            tb_x.Text = br.ReadSingle().ToString();
            tb_y.Text = br.ReadSingle().ToString();
            tb_z.Text = br.ReadSingle().ToString();
            br.BaseStream.Position = ADDR_NAM + offs;
            tmp = br.ReadInt32();
            lb_name2.Text = new string(br.ReadChars(tmp));
            br.Close();
            bt_rest.Enabled = File.Exists(files[cb_saves.SelectedIndex] + ".bak");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_save_Click(object sender, EventArgs e)
    {
        try
        {
            int tmp = cb_saves.SelectedIndex;
            File.Copy(files[tmp], files[tmp] + ".bak", true);
            bw = new BinaryWriter(new FileStream(files[tmp], FileMode.Open, FileAccess.Write));
            bw.BaseStream.Position = ADDR_HPS;
            bw.Write(float.Parse(tb_hp1.Text));
            bw.Write(float.Parse(tb_hp2.Text));
            bw.Write(float.Parse(tb_st1.Text));
            bw.Write(float.Parse(tb_st2.Text));
            bw.Write(float.Parse(tb_dx1.Text));
            bw.Write(float.Parse(tb_dx2.Text));
            bw.Write(float.Parse(tb_pw.Text));
            bw.BaseStream.Position = ADDR_STA;
            for (int i = 0; i < MAX_WP; i++)
                bw.Write(float.Parse(tb_wp[i].Text));
            bw.BaseStream.Position = ADDR_XPK;
            bw.Write(float.Parse(tb_kl.Text));
            bw.Write(float.Parse(tb_xp1.Text));
            bw.Write(int.Parse(tb_xp2.Text));
            bw.Write(int.Parse(tb_pt.Text));
            bw.BaseStream.Position = ADDR_POS + offs;
            bw.Write(float.Parse(tb_x.Text));
            bw.Write(float.Parse(tb_y.Text));
            bw.Write(float.Parse(tb_z.Text));
            bw.Close();
            bt_rest.Enabled = true;
            MessageBox.Show("Mentés módosítva.", TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_rest_Click(object sender, EventArgs e)
    {
        try
        {
            int tmp = cb_saves.SelectedIndex;
            if (File.Exists(files[tmp]))
                File.Delete(files[tmp]);
            File.Move(files[tmp] + ".bak", files[tmp]);
            cb_saves_SelectedIndexChanged(null, null);
            MessageBox.Show("Helyreállítva.", TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_about_Click(object sender, EventArgs e)
    {
        MessageBox.Show("A programot készítette: Kurtis (2016)\nVisual C# 2008 Express-ben íródott.",
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        if (!Directory.Exists(Form1.DIR_SAVE))
        {
            MessageBox.Show("A " + Form1.DIR_SAVE + " mappa nem található! Ezt a programot a Mesterlövész mappájából indítsd!",
                Form1.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }
}