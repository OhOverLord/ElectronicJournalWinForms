using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ElectronicJournal
{
    public partial class Main : Template
    {
        bool isButtonShow = false;
        bool isFirstName = false;
        bool isLastName = false;
        bool isPatronymic = false;
        bool isEmail = false;
        bool isChangePasswordOpen = false;
        public int current = 0;
        List<string> fieldsToUpdate = new List<string>();
        Dictionary<string, string> listFiles = new Dictionary<string, string>();
        string today = DateTime.Today.ToShortDateString();
        Dictionary<string, string[]> marks = new Dictionary<string, string[]>();

        public Main()
        {
            InitializeComponent();
            label2.Text = User.isStuff == 0 ? $"Студент {User.group}" : "Админ";
            label4.Text = User.firstName;
            label5.Text = User.lastName;
            label6.Text = User.email;
            label12.Text = User.patronymic;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.RowHeadersVisible = false;
            dataGridView1.RowHeadersVisible = false;
            openFileDialog1.Filter = "File filter|*.docx; *.doc; *.pdf";
            hideTextBoxes();
            panel4.Hide();
            panel5.Hide();
            label9.Hide();
            if(User.isStuff == 0)
            {
                panel7.Hide();
                panel6.Hide();
                allLessonsGroup(new DB(), User.group, dataGridView2);
                label15.Text = "Все занятия";
                dataGridView1.ReadOnly = true;
                button7.Hide();
            }
            else
            {
                allLessonsGroup(new DB(), "", dataGridView2, true);
                label15.Text = "Все занятия";
            }
            showJournalGroup(dataGridView2.Rows[current].Cells["Группа"].Value.ToString(), dataGridView2.Rows[current].Cells["Предмет"].Value.ToString());
            getFiles(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value), listFiles);
        }
        public void hideTextBoxes()
        {
            textBox1.Hide();
            textBox2.Hide();
            textBox3.Hide();
            textBox6.Hide();
            button1.Hide();
        }

        public void editProfile(TextBox textbox, Label label)
        {
            textbox.Text = label.Text;
            label.Hide();
            textbox.Show();
            if (!isButtonShow)
                button1.Show();
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            editProfile(textBox1, label4);
            isFirstName = true;
        }

        private void label5_DoubleClick(object sender, EventArgs e)
        {
            editProfile(textBox2, label5);
            isLastName = true;
        }

        private void label6_DoubleClick(object sender, EventArgs e)
        {
            editProfile(textBox3, label6);
            isEmail = true;
        }

        int getDatesLessons(DB db, List<string> datesLessons, string group, string subject)
        {
            string sqlQuery = $@"
                select lesson_date
                from dbo.Lesson inner join dbo.Subject
	                on subject_id = dbo.Subject.id
	                inner join dbo.[Group]
		                on group_id = dbo.[Group].id
                where group_title = '{group}' and dbo.Subject.title = '{subject}' 
                order by lesson_date asc;
            ";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while(reader.Read())
                    datesLessons.Add(reader.GetValue(0).ToString().Split(new char[] { ' ' })[0]);
                return datesLessons.Count;
            }
            else
            {
                MessageBox.Show("Error");
            }
            db.closeConnection();
            return 0;
        }

        int getStudents(DB db, List<string> students, string group)
        {
            string sqlQuery = $@"
                select first_name, last_name, patronymic, dbo.[User].id
                from dbo.[User] inner join dbo.[Group]
	                on group_id = dbo.[Group].id
                where group_title = '{group}'
                order by first_name, last_name, patronymic;
            ";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                    students.Add($"{reader.GetValue(0)} {reader.GetValue(1)} {reader.GetValue(2)}|{reader.GetValue(3)}");
                return students.Count;
            }
            else
            {
                MessageBox.Show("Error");
            }
            db.closeConnection();
            return 0;
        }

        public void clearGrid(DataGridView dvg)
        {
            dvg.Rows.Clear();
            dvg.Columns.Clear();
            dvg.Refresh();
        }

        void showMarks(DB db, string group, string subject_id)
        {
            string sqlQuery =  $@"
                select student_id, subject_id, lesson_date, [value]
                from dbo.Mark inner join dbo.Lesson
	                on lesson_id = dbo.Lesson.id
		                inner join dbo.[Subject]
			                on subject_id = dbo.[Subject].id
				                inner join dbo.[Group]
					                on group_id = dbo.[Group].id
                where group_title = '{group}' and subject_id = {subject_id};
            ";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string student_id = reader.GetValue(0).ToString();
                    string cur_subject_id = reader.GetValue(1).ToString();
                    string lesson_date = reader.GetValue(2).ToString().Split(new char[] { ' ' })[0];
                    int mark = reader.GetInt32(3);
                    int i, j;
                    for (i = 0; i < dataGridView1.Rows.Count; i++)
                        if (dataGridView1.Rows[i].Cells[0].Value.ToString() == student_id)
                            break;
                    for (j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if (dataGridView1.Columns[j].HeaderText == lesson_date)
                            break;
                    }
                    if (i != dataGridView1.Rows.Count && j != dataGridView1.ColumnCount)
                        if (mark != 0 && mark != 1)
                            dataGridView1.Rows[i].Cells[j].Value = mark;
                        else if(mark == 1)
                            dataGridView1.Rows[i].Cells[j].Value = "Н";
                        else
                            dataGridView1.Rows[i].Cells[j].Value = "";
                }
            }
            db.closeConnection();
        }

        private void showJournalGroup(string group, string subject)
        {
            clearGrid(dataGridView1);
            List<string> datesLessons = new List<string>();
            List<string> students = new List<string>();
            int countDates = getDatesLessons(new DB(), datesLessons, group, subject);
            int countStudents = getStudents(new DB(), students, group);
            dataGridView1.Columns.Add("", "student_id");
            dataGridView1.Columns.Add("", "ФИО");
            for (int i = 0; i < countDates; i++)
                dataGridView1.Columns.Add("", $"{datesLessons[i]}");

            for (int j = 0; j < countStudents; j++)
            {
                dataGridView1.Rows.Add("", "");
                dataGridView1.Rows[j].Cells[0].Value = $"{students[j].Split(new char[] { '|' })[1]}";
                dataGridView1.Rows[j].Cells[1].Value = $"{students[j].Split(new char[] { '|' })[0]}";
            }
            dataGridView1.Columns[0].Visible = false;
            showMarks(new DB(), dataGridView2.Rows[current].Cells["Группа"].Value.ToString(), dataGridView2.Rows[current].Cells["subject_id"].Value.ToString());
        }

        private void allLessonsGroup(DB db, string group, DataGridView dgv, bool isvisible = false)
        {
            db.openConnection();
            string sqlQuery = "select dbo.[Lesson].id as id, dbo.[Subject].id as subject_id, dbo.[Lesson].title as 'Название занятия', dbo.Subject.title as 'Предмет', group_title as 'Группа', description as 'Описание', lesson_date as Дата " +
                "from dbo.[Lesson] inner join dbo.[Subject] " +
                "on subject_id = dbo.Subject.id " +
                "inner join dbo.[Group] " +
                "on group_id = dbo.[Group].id ";
            if (group == "")
                sqlQuery += "order by lesson_date desc;";
            else
                sqlQuery += $"where group_title = '{group}' order by lesson_date desc;";
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dgv.DataSource = ds.Tables[0];
                dgv.Columns["id"].Visible = false;
                dgv.Columns["subject_id"].Visible = false;
                dgv.Columns["Группа"].Visible = isvisible;
                dgv.Columns["Описание"].Visible = false;

                
                for(int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (dgv.Rows[i].Cells["Дата"].Value.ToString().Split(new char[] { ' ' })[0] == today)
                    {
                        current = i;
                        break;
                    }
                }
                if (group == "")
                    group = dgv.Rows[current].Cells["Группа"].Value.ToString();
                dgv.CurrentCell = dgv["Название занятия", current];
                getLesson(new DB(), Convert.ToInt32(dgv.Rows[current].Cells["id"].Value));
            }
            catch
            {
                MessageBox.Show("Error!");
            }
            db.closeConnection();
        }

        private void getLesson(DB db, int id)
        {
            db.openConnection();
            string sqlQuery = "select dbo.[Lesson].title, dbo.Subject.title, group_title, description " +
                "from dbo.[Lesson] inner join dbo.[Subject] " +
                "on subject_id = dbo.Subject.id " +
                "inner join dbo.[Group] " +
                "on group_id = dbo.[Group].id " +
                $"where dbo.[Lesson].id = {id};";
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                label13.Text = reader.GetValue(1).ToString();
                label14.Text = reader.GetValue(0).ToString();
                textBox7.Text = reader.GetValue(3).ToString();
            }
            else
            {
                label13.Text = "Занятие";
                label14.Text = "Занятие отсутствует";
                textBox7.Text = "Занятие на сегодня отсутствует :)";
            }
            db.closeConnection();
        }
        
        private void updateUser(DB db)
        {
            if (isFirstName)
            {
                fieldsToUpdate.Add($"first_name = '{textBox1.Text}'");
                User.firstName = textBox1.Text;
                isFirstName = false;
            }
            if (isLastName)
            {
                fieldsToUpdate.Add($"last_name = '{textBox2.Text}'");
                User.lastName = textBox2.Text;
                isLastName = false;
            }
            if (isEmail)
            {
                fieldsToUpdate.Add($"email = '{textBox3.Text}'");
                User.email = textBox3.Text;
                isEmail = false;
            }
            if (isPatronymic)
            {
                fieldsToUpdate.Add($"patronymic = '{textBox6.Text}'");
                User.patronymic = textBox6.Text;
                isPatronymic = false;
            }
            string sqlQueryToUpdate = string.Join(", ", fieldsToUpdate);
            string sqlUpdateUser = $"update dbo.[User] set " + sqlQueryToUpdate + " " +
                $"where email = '{label6.Text}'";
            try
            {
                db.openConnection();
                SqlCommand command = new SqlCommand(sqlUpdateUser, db.getConnection());
                command.ExecuteNonQuery();
                db.closeConnection();
                hideTextBoxes();
            }
            catch
            {
                MessageBox.Show("Ошибка на стороне БД");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            updateUser(db);
            label4.Text = User.firstName;
            label5.Text = User.lastName;
            label6.Text = User.email;
            label12.Text = User.patronymic;
            label4.Show();
            label5.Show();
            label6.Show();
            label12.Show();
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            label3.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.ForeColor = Color.Black;
        }

        private void changePasswordOpen()
        {
            panel5.Show();
            panel4.Show();
            textBox4.Text = "";
            textBox5.Text = "";
        }

        private void changePasswordClose()
        {
            panel5.Hide();
            panel4.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if(!isChangePasswordOpen)
            {
                changePasswordOpen();
                isChangePasswordOpen = true;
            }
            else
            {
                changePasswordClose();
                isChangePasswordOpen = false;
            }
        }

        private void changePassQuery(DB db)
        {
            db.openConnection();
            string newPass = User.makePassword(textBox5.Text);
            string sqlChangePassQuery = $"update dbo.[User] set password = '{newPass}' where email = '{User.email}'";
            SqlCommand command = new SqlCommand(sqlChangePassQuery, db.getConnection());
            int n = command.ExecuteNonQuery();
            if (n > 0)
            {
                MessageBox.Show("Пароль поменян успешно");
                User.password = newPass;
            }
            else
            {
                MessageBox.Show("Error");
                return;
            }
            db.closeConnection();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (User.password == User.makePassword(textBox4.Text))
            {
                DB db = new DB();
                changePassQuery(db);
                changePasswordClose();
                isChangePasswordOpen = false;
            }
            else
            {
                label9.Show();
                label9.ForeColor = Color.Red;
                label9.Text = "Неправильный старый пароль";
            }
        }

        private void label10_MouseEnter(object sender, EventArgs e)
        {
            label10.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label10_MouseLeave(object sender, EventArgs e)
        {
            label10.ForeColor = Color.Black;
        }

        private void label10_Click(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.Show();
            this.Close();
        }

        private void label12_DoubleClick(object sender, EventArgs e)
        {
            editProfile(textBox6, label12);
            isPatronymic = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Students changeStudentsForm = new Students();
            this.Close();
            changeStudentsForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Subjects subjectsForm = new Subjects();
            subjectsForm.Show();
            this.Close();
        }

        private void dataGridView2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            current = dataGridView2.SelectedCells[0].RowIndex;
            getLesson(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value));
            showJournalGroup(dataGridView2.Rows[current].Cells["Группа"].Value.ToString(), dataGridView2.Rows[current].Cells["Предмет"].Value.ToString());
            getFiles(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value), listFiles);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView2.RowCount; i++)
                if (dataGridView2["Дата", i].FormattedValue.ToString().Contains(textBox8.Text.Trim()))
                {
                    dataGridView2.CurrentCell = dataGridView2["Дата", i];
                    current = i;
                    getLesson(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value));
                    return;
                }
        }

        void addMarks(DB db)
        {
            db.openConnection();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter($"select * from dbo.Mark", db.getConnection());
            DataSet dataSet = new DataSet();
            SqlCommandBuilder sqlBilder = new SqlCommandBuilder(sqlDataAdapter);
            sqlBilder.GetUpdateCommand();
            sqlDataAdapter.Fill(dataSet, "Marks");
            foreach (KeyValuePair<string, string[]> keyValue in marks)
            {
                DataRow row = dataSet.Tables["Marks"].NewRow();
                row["student_id"] = keyValue.Key.Split(new char[] { '|' })[0];
                row["lesson_id"] = keyValue.Value[0];
                row["value"] = keyValue.Value[1];
                dataSet.Tables["Marks"].Rows.Add(row);
            }
            sqlDataAdapter.Update(dataSet, "Marks");
            marks.Clear();
            db.closeConnection();
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string lesson_id = dataGridView2.Rows[current].Cells["id"].Value.ToString();
            string data = dataGridView1.Columns[e.ColumnIndex].HeaderCell.Value.ToString();
            string student_id = dataGridView1[0, e.RowIndex].Value.ToString();
            if (data != dataGridView2.Rows[current].Cells["Дата"].Value.ToString().Split(new char[] { ' ' })[0])
            {
                MessageBox.Show("Оценки можно ставить только на выбраный день");
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = "";
                return;
            }
            string markValue = "";
            int mark;
            if (dataGridView1[e.ColumnIndex, e.RowIndex].Value != null)
                markValue = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
            else
            {
                markValue = "";
                dataGridView1[e.ColumnIndex, e.RowIndex].Value = "";
            }

            if (markValue != "н" && markValue != "Н" && markValue != "")
            {
                bool result = int.TryParse(markValue, out mark);
                if (result == false)
                {
                    MessageBox.Show("Оценка должна быть цифрой от 2х до 5");
                    dataGridView1[e.ColumnIndex, e.RowIndex].Value = "";
                    return;
                }
                else if (mark < 2 || mark > 5)
                {
                    MessageBox.Show("Оценка должна быть от 2х до 5");
                    dataGridView1[e.ColumnIndex, e.RowIndex].Value = "";
                    return;
                }
            }
            else if (markValue == "")
            {
                mark = 0;
            }
            else
                mark = 1;
            marks.Add($"{student_id}|{data}", new string[] { lesson_id, mark.ToString() });
        }

        private void dataGridView1_Leave(object sender, EventArgs e)
        {
            if (marks.Count > 0)
                addMarks(new DB());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Statistics statisticsForm = new Statistics();
            statisticsForm.Show();
            this.Close();
        }

        void addFile(DB db, int lesson_id, string title, string file)
        {
            string sqlQuery = $@"
                use Journal;
                insert into dbo.[File] ([lesson_id], [title], [file])
                values ({lesson_id}, '{title}', '{file}');
            ";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            int n = command.ExecuteNonQuery();
            if (n > 0)
            {
                db.closeConnection();
                return;
            }
            else
            {
                MessageBox.Show("Error");
                db.closeConnection();
                return;
            }
        }

        void getFiles(DB db, int lesson_id, Dictionary<string, string> listFiles)
        {
            listFiles.Clear();
            string sqlQuery = $@"
                use Journal;
                select [title], [file]
                from dbo.[File]
                where lesson_id = {lesson_id}
            ";
            db.openConnection();
            SqlCommand command = new SqlCommand(sqlQuery, db.getConnection());
            SqlDataReader reader = command.ExecuteReader();
            int i = 0;
            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    listBox1.Items.Insert(i++, reader.GetValue(0));
                    listFiles.Add(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                }
            }
            db.closeConnection();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filePath = openFileDialog1.FileName;
            string[] fileTitle = filePath.Split(new char[] { '\\' });
            listBox1.Items.Clear();
            addFile(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value), fileTitle[fileTitle.Length - 1], filePath);
            getFiles(new DB(), Convert.ToInt32(dataGridView2.Rows[current].Cells["id"].Value), listFiles);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = listFiles[listBox1.SelectedItem.ToString()];
            Process.Start(selectedFile);
        }
    }
}
