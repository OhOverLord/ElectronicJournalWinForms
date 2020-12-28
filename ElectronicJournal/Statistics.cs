using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ElectronicJournal
{
    public partial class Statistics : Template
    {
        public int curGroup;
        public int curSubject;
        public Statistics()
        {
            InitializeComponent();
            getGroups(new DB());
            setSettingsGrid(dataGridView1);
            setSettingsGrid(dataGridView2);
            setSettingsGrid(dataGridView3);
            dataGridView2.Enabled = false;
            label8.Text = "";
            label9.Text = "";
            label10.Text = "";
        }

        void setSettingsGrid(DataGridView dgv)
        {
            dgv.AllowUserToAddRows = false;
            dgv.RowHeadersVisible = false;
        }

        private void getSubjects(DB db, int group_id)
        {
            string sqlQuery = $@"
                select id as subject_id, title as 'Название предмета'
                from dbo.Subject
                where group_id = {group_id};
            ";
            db.openConnection();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            dataGridView2.DataSource = ds.Tables[0];
            dataGridView2.Columns["subject_id"].Visible = false;
            db.closeConnection();
        }

        private void getGroups(DB db)
        {
            string sqlQuery = "select id as group_id, group_title as Группа from dbo.[Group]";
            db.openConnection();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            dataGridView1.DataSource = ds.Tables[0];
            dataGridView1.Columns["group_id"].Visible = false;
            db.closeConnection();
        }

        private void getStudents(DB db, int subject, int group)
        {
            string sqlQuery = $@"
                select dbo.[User].id as student_id, concat_ws(' ', first_name, last_name, patronymic) as ФИО, avg(value) as 'Средний балл'
                from dbo.[Mark] inner join dbo.[User]
	                on student_id = dbo.[User].id
		                inner join dbo.Lesson
			                on lesson_id = dbo.Lesson.id
				                inner join dbo.Subject
					                on subject_id = dbo.Subject.id
				where value <> 0 and value <> 1
                group by dbo.[User].id, student_id, first_name, last_name, patronymic, subject_id, dbo.Subject.group_id
                having subject_id = {subject} and dbo.Subject.group_id = {group}
                order by student_id;	
            ";
            db.openConnection();
            SqlDataAdapter adapter = new SqlDataAdapter(sqlQuery, db.getConnection());
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            dataGridView3.DataSource = ds.Tables[0];
            dataGridView3.Columns["student_id"].Visible = false;
            db.closeConnection();
        }

        void showDiagram()
        {
            chart1.Series["Series1"].Points.Clear();
            int excellentMarks = 0;
            int goodMarks = 0;
            int badMarks = 0;
            for(int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                if (Convert.ToDouble(dataGridView3.Rows[i].Cells["Средний балл"].Value) > 4.5)
                    excellentMarks += 1;
                else if (Convert.ToDouble(dataGridView3.Rows[i].Cells["Средний балл"].Value) > 3.5)
                    goodMarks += 1;
                else
                    badMarks += 1;
            }
            label8.Text = excellentMarks.ToString();
            label8.ForeColor = Color.Green;
            label9.Text = goodMarks.ToString();
            label9.ForeColor = Color.FromArgb(250, 224, 42);
            label10.Text = badMarks.ToString();
            label10.ForeColor = Color.FromArgb(250, 101, 56);
            chart1.Series["Series1"].Points.AddXY("", 0);
            chart1.Series["Series1"].Points.AddXY("", 0);
            chart1.Series["Series1"].Points.AddXY("", 0);
            chart1.Series["Series1"].Points[0].Color = Color.Green;
            chart1.Series["Series1"].Points[1].Color = Color.FromArgb(250, 224, 42);
            chart1.Series["Series1"].Points[2].Color = Color.FromArgb(250, 101, 56);
            chart1.Series["Series1"].Points[0].SetValueXY("Отличники", excellentMarks);
            chart1.Series["Series1"].Points[1].SetValueXY("Хорошисты", goodMarks);
            chart1.Series["Series1"].Points[2].SetValueXY("Троечники", badMarks);
        }

        private void label4_MouseEnter(object sender, EventArgs e)
        {
            label4.ForeColor = Color.FromArgb(123, 195, 195);
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            label4.ForeColor = Color.Black;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Main mainForm = new Main();
            mainForm.Show();
            this.Close();
        }

        public void clearGrid(DataGridView dvg)
        {
            dvg.Columns.Clear();
            dvg.Refresh();
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            curGroup = dataGridView1.SelectedCells[0].RowIndex;
            getSubjects(new DB(), Convert.ToInt32(dataGridView1.Rows[curGroup].Cells["group_id"].Value));
            dataGridView2.Enabled = true;
            clearGrid(dataGridView3);
        }

        private void dataGridView2_Click(object sender, EventArgs e)
        {
            curSubject = dataGridView2.SelectedCells[0].RowIndex;
            getStudents(new DB(), Convert.ToInt32(dataGridView2.Rows[curSubject].Cells["subject_id"].Value), 
                Convert.ToInt32(dataGridView1.Rows[curGroup].Cells["group_id"].Value));
            showDiagram();
        }
    }
}
