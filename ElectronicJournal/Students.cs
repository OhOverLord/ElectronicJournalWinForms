using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ElectronicJournal
{
    public partial class Students : Template
    {
        public int selectedRow = 0;
        public Students()
        {
            InitializeComponent();
            dataGridView1.Height = panel6.Height - 100;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            DB db = new DB();
            getAllStudents(db);
            fillStudentCard(selectedRow);
        }

        public void fillStudentCard(int row)
        {
            email_field.Text = dataGridView1.Rows[row].Cells["email"].Value.ToString();
            first_name_field.Text = dataGridView1.Rows[row].Cells["Имя"].Value.ToString();
            last_name_field.Text = dataGridView1.Rows[row].Cells["Фамилия"].Value.ToString();
            patronymic_field.Text = dataGridView1.Rows[row].Cells["Отчество"].Value.ToString();
            group_field.Text = dataGridView1.Rows[row].Cells["Группа"].Value.ToString();
            password_field.Text = "********";
        }

        private void getAllStudents(DB db)
        {
            db.getConnection();
            string sqlQuery = "select " +
                "dbo.[User].id as id, " +
                "first_name as 'Имя', " +
                "last_name as 'Фамилия', " +
                "patronymic as 'Отчество', " +
                "group_title as 'Группа', " +
                "password, " +
                "email, " +
                "is_stuff " +
                "from dbo.[User] inner join dbo.[Group] " +
                "on group_id = dbo.[Group].id " +
                "where is_stuff = 0";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dataGridView1.DataSource = ds.Tables[0];
                dataGridView1.Columns["id"].Visible = false;
                dataGridView1.Columns["password"].Visible = false;
                dataGridView1.Columns["is_stuff"].Visible = false;
                dataGridView1.CurrentCell = dataGridView1["Имя", selectedRow];
            }
            catch
            {
                MessageBox.Show("Error!");
            }
            db.closeConnection();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Main mainForm = new Main();
            this.Close();
            mainForm.Show();
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            label3.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.ForeColor = Color.Black;
        }
        
        private void panel6_Resize(object sender, EventArgs e)
        {
            dataGridView1.Height = panel6.Height - 139;
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            selectedRow = dataGridView1.SelectedCells[0].RowIndex;
            fillStudentCard(selectedRow);
        }

        private void addStudent(DB db)
        {
            db.openConnection();
            string password = User.makePassword(password_field.Text);
            string sqlQuery = "insert into dbo.[User] " +
                "(email, first_name, last_name, is_stuff, group_id, password, patronymic) " +
                $"values('{email_field.Text}', " +
                $"'{first_name_field.Text}', " +
                $"'{last_name_field.Text}', " +
                $"0, " +
                $"(select id from dbo.[Group] group by group_title, id having group_title = '{group_field.Text}'), " +
                $"'{password}', " +
                $"'{patronymic_field.Text}')";
            try
            {
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show("Студент добавлен");
                    getAllStudents(db);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void deleteStudent(DB db)
        {
            db.openConnection();
            string sqlQuery = $"delete from dbo.[User] where id = {dataGridView1.Rows[selectedRow].Cells["id"].Value}";
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            int n = command.ExecuteNonQuery();
            try
            {
                if (n > 0)
                {
                    MessageBox.Show("Студент удалён");
                    selectedRow = 0;
                    getAllStudents(db);
                    fillStudentCard(selectedRow);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void updateStudent(DB db, string fields)
        {
            db.openConnection();
            string sqlQuery = "update dbo.[User] set " + fields + " " + $"where id = {dataGridView1.Rows[selectedRow].Cells["id"].Value}";
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            int n = command.ExecuteNonQuery();
            try
            {
                if (n > 0)
                {
                    MessageBox.Show("Студент изменён");
                    getAllStudents(db);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button4.Hide();
            button5.Hide();
            email_field.Text = "";
            first_name_field.Text = "";
            last_name_field.Text = "";
            patronymic_field.Text = "";
            group_field.Text = "";
            password_field.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            if (group_field.Text == "")
            {
                MessageBox.Show("Введите группу");
                return;
            }
            addStudent(db);
            fillStudentCard(selectedRow);
            button4.Show();
            button5.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            deleteStudent(db);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (group_field.Text == "")
            {
                MessageBox.Show("Введите группу");
                return;
            }
            List<string> fieldsToUpdate = new List<string>();
            string oldPass = User.makePassword(dataGridView1.Rows[selectedRow].Cells["password"].Value.ToString());
            string newPass = User.makePassword(password_field.Text);
            if(email_field.Text != dataGridView1.Rows[selectedRow].Cells["email"].Value.ToString())
                fieldsToUpdate.Add($"email = '{email_field.Text}'");
            if (first_name_field.Text != dataGridView1.Rows[selectedRow].Cells["Имя"].Value.ToString())
                fieldsToUpdate.Add($"first_name = '{first_name_field.Text}'");
            if (last_name_field.Text != dataGridView1.Rows[selectedRow].Cells["Фамилия"].Value.ToString())
                fieldsToUpdate.Add($"last_name = '{last_name_field.Text}'");
            if (patronymic_field.Text != dataGridView1.Rows[selectedRow].Cells["Отчество"].Value.ToString())
                fieldsToUpdate.Add($"patronymic = '{patronymic_field.Text}'");
            if (group_field.Text != dataGridView1.Rows[selectedRow].Cells["Группа"].Value.ToString())
                fieldsToUpdate.Add($"group_id = (select id from dbo.[Group] group by group_title, id having group_title = '{group_field.Text}')");
            if (oldPass != newPass)
                fieldsToUpdate.Add($"password = '{newPass}'");

            DB db = new DB();
            updateStudent(db, string.Join(", ", fieldsToUpdate));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
                if (dataGridView1["email", i].FormattedValue.ToString().Contains(textBox1.Text.Trim()))
                {
                    dataGridView1.CurrentCell = dataGridView1["email", i];
                    selectedRow = i;
                    fillStudentCard(selectedRow);
                    return;
                }
        }

        private void label10_Click(object sender, EventArgs e)
        {
            password_field.Text = User.GetRandomPassword();
        }

        private void label10_MouseEnter(object sender, EventArgs e)
        {
            label10.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label10_MouseLeave(object sender, EventArgs e)
        {
            label10.ForeColor = Color.Black;
        }
    }
}
