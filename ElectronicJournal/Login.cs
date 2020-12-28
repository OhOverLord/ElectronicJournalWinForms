using System;
using System.Drawing;
using System.Data.SqlClient;

namespace ElectronicJournal
{
    public partial class Login : Template
    {
        public Login()
        {
            InitializeComponent();
            label5.Text = "";
            textBox1.Text = "foo@example.com";
            textBox2.Text = "********";
            textBox1.ForeColor = Color.Gray;
            textBox2.ForeColor = Color.Gray;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "foo@example.com" || textBox2.Text == "********")
            {
                label5.Text = "Заполните поля";
                label5.ForeColor = Color.Red;
                label2.ForeColor = Color.Red;
                label3.ForeColor = Color.Red;
                return;
            }
            string passwordHash = User.makePassword(textBox2.Text);
            string sqlGetUser = $"select * from dbo.[User] where email = '{textBox1.Text}' and password = '{passwordHash}'";
            DB db = new DB();
            db.openConnection();
            try
            {
                SqlCommand command = new SqlCommand(sqlGetUser, db.getConnection());
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                    reader.Read();
                User.email = reader.GetValue(1).ToString();
                User.firstName = reader.GetValue(2).ToString();
                User.lastName = reader.GetValue(3).ToString();
                User.isStuff = int.Parse(reader.GetValue(4).ToString());
                User.password = passwordHash;
                User.patronymic = reader.GetValue(7).ToString();
            }
            catch
            {
                label5.Text = "Неправильный логин или пароль";
                label5.ForeColor = Color.Red;
                textBox2.Text = "";
                label2.ForeColor = Color.Red;
                label3.ForeColor = Color.Red;
                return;
            }
            db.closeConnection();
            db.openConnection();
            if (User.isStuff == 0)
            {
                string sqlGetGroup = $"select group_title " +
                    $"from dbo.[Group] inner join dbo.[User] " +
                    $"on dbo.[Group].id = dbo.[User].group_id " +
                    $"where dbo.[User].email = '{User.email}' and dbo.[User].password = '{User.password}'";
                SqlCommand commandGetGroup = new SqlCommand(sqlGetGroup, db.getConnection());
                SqlDataReader readerGetGroup = commandGetGroup.ExecuteReader();
                if (readerGetGroup.HasRows)
                    readerGetGroup.Read();
                User.group = readerGetGroup.GetValue(0).ToString();
            }
            db.closeConnection();
            Main mainWindow = new Main();
            mainWindow.Show();
            this.Hide();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "foo@example.com")
                textBox1.Text = "";
            textBox1.ForeColor = Color.Black;
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "foo@example.com";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "********")
                textBox2.Text = "";
            textBox2.ForeColor = Color.Black;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = "********";
                textBox2.ForeColor = Color.Gray;
            }
        }
    }
}
