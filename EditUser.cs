﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlServerCe;

namespace PDA_1._0
{
    public partial class EditUser : Form
{
        private SqlCeConnection cn;
        string username;
        private List<Site> sites = null;
        private int role;
        private string currentPass;

        private SqlCeConnection usersCN;
        private System.Windows.Forms.MenuItem menuItemEditAdmin;

        public EditUser(int role,string name)
        {
            this.role = role;
            username = name;
            InitializeComponent();
            this.menuItemEditUser.Enabled = false;
            fillComboBoxListUsers();

            if (role == 9)
            {
                this.menuItem1.MenuItems.Remove(menuItemEditPass);
                menuItemEditAdmin = new System.Windows.Forms.MenuItem();
                this.menuItem1.MenuItems.Add(menuItemEditAdmin);
                menuItemEditAdmin.Text = "Modifier l'ADMIN";
                menuItemEditAdmin.Click += new System.EventHandler(this.menuItemEditAdmin_Click);
            
            }

        }

        private void menuItemConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openfile1 = new OpenFileDialog();
                openfile1.Filter = "CSV (*.csv)|*.csv";

                if (openfile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var reader = new StreamReader(File.OpenRead(@"" + openfile1.FileName));
                    List<List<string>> listA = new List<List<string>>();


                    List<string> listVals;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        listVals = splitString(line);
                        listA.Add(listVals);
                    }
                    openfile1.Dispose();
                    reader.Close();
                    reader.Dispose();

                    //editableDataGrid.ColumnHeadersVisible = false;
                    //editableDataGrid.DataSource = CreateDataTable(listVals, listA);
                    insertConfigToDB(listA);
                }
            }
            catch { }
        }

        private void menuItemLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("voulez vous vraiment vous déconnecter ?", "Déconnexion", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                var form2 = new Login(username);
                form2.Closed += (s, args) => this.Close();
                form2.Show();
                this.Hide();
            }

        }
         
        private void menuItemAddUser_Click(object sender, EventArgs e)
        {
            var Form = new AddUser(role, username);
            Form.Closed += (s, args) => this.Close();
            Form.Show();
            this.Hide();
        }


        private void menuItemEditPass_Click(object sender, EventArgs e)
        {
            var Form = new EditPass(role, username);
            Form.Closed += (s, args) => this.Close();
            Form.Show();
            this.Hide();
        }

        private void menuItemImportProduits_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openfile1 = new OpenFileDialog();
                openfile1.Filter = "CSV (*.csv)|*.csv";

                if (openfile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var reader = new StreamReader(File.OpenRead(@"" + openfile1.FileName));
                    List<List<string>> listA = new List<List<string>>();


                    List<string> listVals;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        listVals = splitString(line);
                        listA.Add(listVals);
                    }
                    openfile1.Dispose();
                    reader.Close();
                    reader.Dispose();

                    insertProduitsToDB(listA);
                }
            }
            catch { }
        }

        private void menuItemImporterCSV_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openfile1 = new OpenFileDialog();
                openfile1.Filter = "CSV (*.csv)|*.csv";

                if (openfile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var reader = new StreamReader(File.OpenRead(@"" + openfile1.FileName));
                    List<List<string>> listA = new List<List<string>>();


                    List<string> listVals = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        listVals = splitString(line);

                        listA.Add(listVals);
                    }
                    openfile1.Dispose();
                    reader.Close();
                    reader.Dispose();

                    //editableDataGrid.ColumnHeadersVisible = false;
                    //editableDataGrid.DataSource = CreateDataTable(listVals, listA);
                    insertListToDB(listA);

                }
            }
            catch { }
        }

        private void menuItemRresetDB_Click(object sender, EventArgs e)
        {
            CreateDataBase();
        }

        private List<string> splitString(string stringToSplit)
        {
            char[] characters = stringToSplit.ToCharArray();
            List<string> returnValueList = new List<string>();
            string tempString = "";
            bool blockUntilEndQuote = false;
            int characterCount = 0;
            foreach (char character in characters)
            {
                characterCount = characterCount + 1;

                if (character == '"')
                {
                    if (blockUntilEndQuote == false)
                    {
                        blockUntilEndQuote = true;
                    }
                    else if (blockUntilEndQuote == true)
                    {
                        blockUntilEndQuote = false;
                    }
                }

                if (character != ',')
                {
                    tempString = tempString + character;
                }
                else if (character == ',' && blockUntilEndQuote == true)
                {
                    tempString = tempString + character;
                }
                else
                {
                    tempString = tempString.TrimEnd('"').TrimStart('"');
                    returnValueList.Add(tempString);
                    tempString = "";
                }

                if (characterCount == characters.Length)
                {
                    tempString = tempString.TrimEnd('"').TrimStart('"');
                    //var tmp = tempString.Substring(0, tempString.Length - 1).EndsWith(".") ? tempString.Substring(0, tempString.Length - 3) : tempString;
                    returnValueList.Add(tempString);
                    tempString = "";
                }
            }

            return returnValueList;
        }

        public void CreateDataBase()
        {
            if (File.Exists("myDb.sdf"))
                File.Delete("myDb.sdf");

            string connStr = "Data Source = myDb.sdf; Password = <password>";

            SqlCeEngine engine = new SqlCeEngine(connStr);
            engine.CreateDatabase();
            engine.Dispose();

            SqlCeConnection conn = null;

            try
            {
                conn = new SqlCeConnection(connStr);
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE liaison (idLiaison int,nom ntext, siteA int, siteB int,configRadio ntext)";
                cmd.ExecuteNonQuery();

                SqlCeCommand cmd2 = conn.CreateCommand();
                cmd2.CommandText = "CREATE TABLE site (idSite int, nom ntext)";
                cmd2.ExecuteNonQuery();

                SqlCeCommand cmd3 = conn.CreateCommand();
                cmd3.CommandText = "CREATE TABLE article (idArticle int, nom ntext, config ntext, type ntext, designation ntext)";
                cmd3.ExecuteNonQuery();

                SqlCeCommand cmd4 = conn.CreateCommand();
                // UC parent (ucParent)- Parent S/N (parentSN) _ UC (UC)
                // Numero de serie (numSerie) _ Code immo (immo)
                cmd4.CommandText = "CREATE TABLE produits (ucParent ntext, parentSN ntext, UC ntext,numSerie ntext, immo ntext)";
                cmd4.ExecuteNonQuery();
            }
            catch { }
            finally
            {
                conn.Close();
            }
            MessageBox.Show("Base de données vide créée avec succès");
        }

        private void insertConfigToDB(List<List<string>> list)
        {
            beginConnection();
            int colonne;
            int ligne = 0;
            string type = "";
            string config = "";
            string article = "";
            string designation = "";
            //insert Sites
            foreach (List<string> L in list)
            {
                colonne = 0;
                ligne++;
                if (ligne != 1)
                {
                    foreach (string s in L)
                    {
                        string chaine = s.Replace("'", "''");
                        colonne++;
                        if (colonne == 1)
                        {
                            if (s.Length > 0)
                                type = chaine;
                        }
                        else if (colonne == 2)
                        {
                            if (s.Length > 0)
                                config = chaine;
                        }
                        else if (colonne == 3)
                        {
                            article = chaine;
                        }
                        else if (colonne == 4)
                        {
                            designation = chaine;
                        }
                    }
                    SqlCeCommand cmd = cn.CreateCommand();
                    //MessageBox.Show(type + "','" + config + "','" + article);
                    cmd.CommandText = "INSERT INTO article (nom, config, type, designation) VALUES ('" + article + "','" + config + "','" + type + "','" + designation + "')";
                    cmd.ExecuteNonQuery();
                }
            }

            endConnection();
        }

        private void insertProduitsToDB(List<List<string>> list)
        {
            beginConnection();
            int colonne;
            int ligne = 0;
            string ucParent = "";
            string parentSN = "";
            string UC = "";
            string numSerie = "";
            string immo = "";
            //insert Sites
            foreach (List<string> L in list)
            {
                colonne = 0;
                ligne++;
                if (ligne != 1)
                {
                    foreach (string s in L)
                    {
                        colonne++;
                        if (colonne == 1)
                        {
                            ucParent = s;
                        }
                        else if (colonne == 2)
                        {
                            parentSN = s;
                        }
                        else if (colonne == 3)
                        {
                            UC = s;
                        }
                        else if (colonne == 4)
                        {
                            numSerie = s;
                        }
                        else if (colonne == 5)
                        {
                            immo = s;
                        }

                    }
                    SqlCeCommand cmd = cn.CreateCommand();
                    //MessageBox.Show(ucParent + " - " + parentSN + " - " + UC + " - " + numSerie + " - " + immo);
                    cmd.CommandText = "INSERT INTO produits VALUES ('" + ucParent + "','" + parentSN + "','" + UC + "','" + numSerie + "','" + immo + "')";
                    cmd.ExecuteNonQuery();
                }
            }

            endConnection();
        }

        private void insertListToDB(List<List<string>> list)
        {
            beginConnection();
            int colonne;
            int ligne = 0;

            //insert Sites
            foreach (List<string> L in list)
            {
                colonne = 0;
                ligne++;
                if (ligne != 1)
                {
                    foreach (string s in L)
                    {
                        colonne++;

                        if (colonne == 2 || colonne == 3)
                        {
                            //if (getSiteID(s) == 0)
                            //{
                            SqlCeCommand cmd = cn.CreateCommand();
                            Random r = new Random();
                            int n = r.Next();
                            cmd.CommandText = "INSERT INTO site (idSite,nom) VALUES (" + n + ",'" + s + "')";
                            cmd.ExecuteNonQuery();
                            //}
                        }
                    }
                }
            }

            //insert Liaisons
            ligne = 0;
            string nomLiaison = "";
            string site_a = "";
            string site_b = "";
            string configRadio = "";
            foreach (List<string> L in list)
            {
                colonne = 0;
                ligne++;
                if (ligne != 1)
                {
                    foreach (string s in L)
                    {
                        colonne++;
                        if (colonne == 1)
                            nomLiaison = s;
                        else if (colonne == 2)
                            site_a = s;
                        else if (colonne == 3)
                            site_b = s;
                        else if (colonne == 4)
                            configRadio = s;
                    }

                    SqlCeCommand cmd = cn.CreateCommand();
                    cmd.CommandText = "INSERT INTO liaison (nom, siteA, siteB, configRadio) VALUES ('" + nomLiaison + "'," + getSiteID(site_a) + "," + getSiteID(site_b) + ",'" + configRadio + "')";
                    cmd.ExecuteNonQuery();
                }
            }
            endConnection();
        }

        private void beginConnection()
        {
            try
            {
                cn = new SqlCeConnection("Data Source = myDb.sdf; Password = <password>");
                cn.Open();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show("la connexion à la base de données a échoué :" + ex.Message);

            }
        }

        private void endConnection()
        {
            cn.Close();
        }

        public int getSiteID(string nom)
        {
            if (sites == null)
                sites = ListSites();

            foreach (Site site in sites)
            {
                if (site.Nom.ToLower().Equals(nom.ToLower()))
                    return site.Id;
            }
            return 0;
        }

        public List<Site> ListSites()
        {
            Site site = new Site();
            List<Site> all = new List<Site>();
            SqlCeCommand cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT * FROM site";
            SqlCeDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                site = new Site();
                site.Id = rdr.GetInt32(0);
                site.Nom = rdr.GetString(1);
                all.Add(site);
            }
            rdr.Close();
            return all;
        }

        public void fillComboBoxListUsers()
        {
            this.comboBoxListUsers.Items.Clear();
            beginUsersConnection();
            SqlCeCommand cmd = usersCN.CreateCommand();
            // users (idUser,nom,pass,role) VALUES (1,'admin','admin',1)";

            cmd.CommandText = "SELECT * FROM users where role != 1";
            SqlCeDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                this.comboBoxListUsers.Items.Add(rdr.GetString(1));
            }
            rdr.Close();
            endUsersConnection();
        }

        private void beginUsersConnection()
        {
            try
            {
                usersCN = new SqlCeConnection("Data Source = UsersDb.sdf; Password = @inov@");
                usersCN.Open();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show("la connexion à la base de données a échoué :" + ex.Message);

            }
        }

        private void endUsersConnection()
        {
            usersCN.Close();
        }

        private void comboBoxListUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.buttonShowPass.Enabled == false)
            {
                this.buttonDelete.Enabled = true;
                this.buttonShowPass.Enabled = true;
                this.buttonSave.Enabled = true;
                this.textBoxNewPass1.Enabled = true;
                this.textBoxNewPass2.Enabled = true;
                this.checkBoxNewPass1.Enabled = true;
                this.checkBoxNewPass2.Enabled = true;
            }
            currentPass = getPass(comboBoxListUsers.Text);
        }

        private string getPass(string username)
        {
            beginUsersConnection();
            SqlCeCommand cmd = usersCN.CreateCommand();
            cmd.CommandText = "SELECT * FROM users where nom like '" + username + "'";
            SqlCeDataReader rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                string pass = rdr.GetString(2);
                endUsersConnection();
                return pass;
              
            }
            endUsersConnection();
            return "";
        }

        private void buttonShowPass_Click(object sender, EventArgs e)
        {
            MessageBox.Show("le mot de passe de "+comboBoxListUsers.Text+" est : "+
                currentPass);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (this.textBoxNewPass1.Text.Length > 2)
            {
                if (this.textBoxNewPass1.Text.Equals(this.textBoxNewPass2.Text))
                {
                    changePass(comboBoxListUsers.Text, this.textBoxNewPass1.Text);
                    MessageBox.Show("Vous avez changer le mot de passe de " + comboBoxListUsers.Text + ".");
                    currentPass = getPass(comboBoxListUsers.Text);
                    this.textBoxNewPass1.Text = "";
                    this.textBoxNewPass2.Text = "";
                }
                else
                {
                    this.textBoxNewPass1.Text = "";
                    this.textBoxNewPass2.Text = "";
                    this.textBoxNewPass1.Focus();
                    MessageBox.Show("Les deux mots de passe ne sont pas identiques");
                }
            }
            else
            {
                MessageBox.Show("le mot de passe doit être formé d'au moins 3 caractères");
                this.textBoxNewPass1.Text = "";
                this.textBoxNewPass2.Text = "";
                this.textBoxNewPass1.Focus();
            }
        }

        public void changePass(string username, string newPass)
        {
            beginUsersConnection();
            SqlCeCommand cmd = usersCN.CreateCommand();
            cmd.CommandText = "update users SET pass = '" + newPass + "'" +
                "where nom like '" + username + "'";
            cmd.ExecuteNonQuery();
            endUsersConnection();
        }

        private void buttonRetour_Click(object sender, EventArgs e)
        {
            var home = new Form1(role, username);
            home.Closed += (s, args) => this.Close();
            home.Show();
            this.Hide();
        }

        private void checkBoxNewPass1_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.textBoxNewPass1.PasswordChar.Equals('*'))
                this.textBoxNewPass1.PasswordChar = '\0';
            else
                this.textBoxNewPass1.PasswordChar = '*';
            textBoxNewPass1.Focus();
        }

        private void checkBoxNewPass2_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.textBoxNewPass2.PasswordChar.Equals('*'))
                this.textBoxNewPass2.PasswordChar = '\0';
            else
                this.textBoxNewPass2.PasswordChar = '*';
            textBoxNewPass2.Focus();
        }

        private void menuItemEditAdmin_Click(object sender, EventArgs e)
        {
            var Form = new EditPass(9, "admin");
            Form.Show();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("voulez vous vraiment supprimer l'utilisateur " + comboBoxListUsers.Text + " ? ", "Supression", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                string name = comboBoxListUsers.Text;
                deleteUser(comboBoxListUsers.Text);
                MessageBox.Show("L'utilisateur " + name + " est supprimé avec succès");
                fillComboBoxListUsers();
                comboBoxListUsers.Refresh();
            }
        }

        private void deleteUser(string username)
        {
            beginUsersConnection();
            SqlCeCommand cmd = usersCN.CreateCommand();
            cmd.CommandText = "DELETE FROM users where nom like '" + username + "'";
            cmd.ExecuteNonQuery();
            endUsersConnection();
        }

    }
}