using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ElectronicJournal
{
    public partial class Subjects : Template
    {
        List<string> listSbujects = new List<string>();
        List<string> listGroups = new List<string>();
        string selectedDate = DateTime.Today.ToString();
        int isLesson = 0;
        public Subjects()
        {
            InitializeComponent();
            DB db = new DB();
            getElements(db, "Group", "group_title", listGroups, "");
            comboBox2.Items.AddRange(listGroups.ToArray());
            comboBox2.Text = comboBox2.Items[0].ToString();
            getAllLessons(db);
            getLesson(db, dateTimePicker1.Value.ToString(), comboBox1.Text, comboBox2.Text);
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
        }

        private int getLesson(DB db, string date, string subject, string group)
        {
            int id;
            string sqlQuery = "select id, title, description " +
                "from dbo.Lesson " +
                $"where lesson_date = '{date}' and subject_id in (select id from dbo.Subject where title = '{subject}' and group_id in (select id from dbo.[Group] where group_title = '{group}'))";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                id = int.Parse(reader.GetValue(0).ToString());
                textBox1.Text = reader.GetValue(1).ToString();
                richTextBox1.Text = reader.GetValue(2).ToString();
            }
            else
                return 0;
            db.closeConnection();
            return id;
        }

        private void getAllLessons(DB db)
        {
            db.getConnection();
            string sqlQuery = "select dbo.[Lesson].title as Занятие, " +
                "dbo.[Lesson].lesson_date as Время, " +
                "dbo.[Subject].title as Предмет, " +
                "dbo.[Group].group_title as Группа " +
                "from dbo.[Lesson] inner join dbo.[Subject] " +
                "on subject_id = dbo.[Subject].id " +
                "inner join dbo.[Group] " +
                "on group_id = dbo.[Group].id";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch
            {
                MessageBox.Show("Error!");
            }
            db.closeConnection();
        }

        private void getElements(DB db, string tableName, string fieldName, List<string> arr, string addition)
        {
            string sqlQuery = $"select {fieldName} from dbo.[{tableName}] " + addition;
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                    arr.Add(reader.GetValue(0).ToString());
            db.closeConnection();
        }

        private bool addLesson(DB db, string lesson_date, string title, string subject, string description, string group)
        {
            string sqlQuery = $"insert into dbo.Lesson " +
                $"([lesson_date], title, subject_id, description) " +
                $"values " +
                $"('{lesson_date}', " +
                $"'{title}', " +
                $"(select id from dbo.Subject where title = '{subject}' and group_id in (select id from dbo.[Group] where group_title = '{group}')), " +
                $"'{description}');";
            db.openConnection();
            try
            { 
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show("Занятие добавлено");
                    getAllLessons(db);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return false;
            }
            db.closeConnection();
            return true;
        }

        private void updateLesson(DB db, int id, string title, string description)
        {
            string sqlQuery = $"update dbo.Lesson set title = '{title}', description = '{description}' where id = {id}";
            db.openConnection();
            try
            {
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show("Занятие изменено");
                    getAllLessons(db);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }
        
        private void deleteLesson(DB db, int id)
        {
            string sqlQuery = $"delete from dbo.Lesson where id = {id}";
            db.openConnection();
            try
            {
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show("Удалено");
                    getAllLessons(db);
                }
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void addGroup(DB db, string group)
        {
            string sqlQuery = $"insert into dbo.[Group] (group_title) values ('{group}')";
            db.openConnection();
            try
            {
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                    return;
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void addSubject(DB db, string subject, string group)
        {
            string sqlQuery = $"insert into dbo.[Subject] (title, group_id) values ('{subject}', (select id from dbo.[Group] where group_title = '{group}'))";
            db.openConnection();
            try
            {
                SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
                int n = command.ExecuteNonQuery();
                if (n > 0)
                    return;
            }
            catch
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Main mainForm = new Main();
            mainForm.Show();
            this.Close();
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            label3.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.ForeColor = Color.Black;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!listGroups.Contains(comboBox2.Text))
            {
                string current_group = comboBox1.Text;
                addGroup(new DB(), comboBox2.Text);
                listGroups.Add(comboBox2.Text);
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(listGroups.ToArray());
                comboBox2.Text = current_group;
            }
            else if (!listSbujects.Contains(comboBox1.Text))
            {
                string current_subject = comboBox1.Text;
                addSubject(new DB(), comboBox1.Text, comboBox2.Text);
                listSbujects.Add(comboBox1.Text);
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(listSbujects.ToArray());
                comboBox1.Text = current_subject;
            }

            isLesson = getLesson(new DB(), dateTimePicker1.Value.ToString(), comboBox1.Text, comboBox2.Text);
            if(isLesson == 0)
            {
                textBox1.Text = "";
                richTextBox1.Text = "";
                button5.Enabled = false;
            }
            else
                button5.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (isLesson == 0)
            {
                if (!addLesson(new DB(), dateTimePicker1.Value.ToShortDateString(), textBox1.Text, comboBox1.Text, richTextBox1.Text, comboBox2.Text))
                    return;
                button5.Enabled = true;
            }
            else
            {
                updateLesson(new DB(), isLesson, textBox1.Text, richTextBox1.Text);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            deleteLesson(new DB(), isLesson);
            textBox1.Text = "";
            richTextBox1.Text = "";
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            listSbujects.Clear();
            getElements(new DB(), "Subject", "title", listSbujects, $"where group_id in (select id from dbo.[Group] where group_title = '{comboBox2.Text}')");
            comboBox1.Items.AddRange(listSbujects.ToArray());
            comboBox1.Text = "";
        }
    }
}
