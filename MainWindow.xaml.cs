using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using System.Data.SQLite;
namespace lab1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SQLiteConnection StudentsDBConnection = null;

        List<StudentRow> UncreatedStudentsDataRows = new List<StudentRow>();
        List<StudentRow> UnupdatedStudentsDataRows = new List<StudentRow>();
        List<StudentRow> UndeletedStudentsDataRows = new List<StudentRow>();

        bool HasUnapplyedDataRows = false;
        public MainWindow() {
            InitializeComponent();
            
        }
        public class StudentRow {
            public int Id { get; set; }

            public int IdInDB = -1;
            public string Fullname { get; set; }
            public string MarkMath { get; set; }
            public string MarkPhysics { get; set; }

            public StudentRow(int id, string fullname, string markMath, string markPhysics) {
                Id = id;
                Fullname = fullname;
                MarkMath = markMath;
                MarkPhysics = markPhysics;
            }
        }
        SQLiteConnection ConnectToDB(string path) {
            SQLiteConnection connection;
            connection = new SQLiteConnection($"Data Source={path};Version=3;");
            connection.Open();
            return connection;
        }
        SQLiteDataReader GetStudentsDataReader(ref SQLiteConnection connection , string sqlSelectCommandQuery = "Default") { 
            if (sqlSelectCommandQuery == "Default") {
                sqlSelectCommandQuery = "SELECT * FROM about, marks WHERE about.id = marks.id";
            }

            SQLiteCommand command = new SQLiteCommand(sqlSelectCommandQuery, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;

        }
        void ChangeWindowStyleToUnsaved() {
            if (!HasUnapplyedDataRows) {
                Title += "*";
            }
        }
        void ChangeWindowStyleToSaved() {
            if (HasUnapplyedDataRows) {
                Title = Title.Replace("*", "");
            }
        }
        
        void ApplyChangesToDB(ref SQLiteConnection connection) {
            foreach (StudentRow uncreatedRow in UncreatedStudentsDataRows) {
                InsertRowToDB(uncreatedRow, ref connection);
            }
            UncreatedStudentsDataRows.Clear();
            foreach (StudentRow unupdatedRow in UnupdatedStudentsDataRows) {
                UpdateRowInDB(unupdatedRow, ref connection);
            }
            UnupdatedStudentsDataRows.Clear();
            foreach (StudentRow undeletedRow in UndeletedStudentsDataRows) {
                DeleteRowInDB(undeletedRow, ref connection);
            }
            UndeletedStudentsDataRows.Clear();
            ChangeWindowStyleToSaved();
            HasUnapplyedDataRows = false;
        }
        
        void InsertRowToDB(StudentRow row, ref SQLiteConnection connection) {
            string sqlInsertAboutTableQuery = $"INSERT INTO about (id, fullname) VALUES (\"{row.Id}\", \"{row.Fullname}\")";
            string sqlInsertMarksTableQuery = $"INSERT INTO marks (id, markMath, markPhysics) VALUES (\"{row.Id}\", \"{row.MarkMath}\", \"{row.MarkPhysics}\")";
            new SQLiteCommand(sqlInsertAboutTableQuery, connection).ExecuteNonQuery();
            new SQLiteCommand(sqlInsertMarksTableQuery, connection).ExecuteNonQuery();
        }

        void UpdateRowInDB(StudentRow row, ref SQLiteConnection connection) {
            string sqlUpdateAboutTableQuery = $"UPDATE about SET id = \"{row.Id}\", fullname = \"{row.Fullname}\" WHERE about.id = {row.IdInDB}";
            string sqlUpdateMarksTableQuery = $"UPDATE marks SET id = \"{row.Id}\", markMath = \"{row.MarkMath}\", markPhysics = \"{row.MarkPhysics}\" WHERE marks.id = {row.IdInDB}";
            new SQLiteCommand(sqlUpdateAboutTableQuery, connection).ExecuteNonQuery();
            new SQLiteCommand(sqlUpdateMarksTableQuery, connection).ExecuteNonQuery();
        }

        void DeleteRowInDB(StudentRow row, ref SQLiteConnection connection) {
            string sqlDeleteAboutTableRowQuery = $"DELETE FROM about WHERE id = {row.Id}";
            string sqlDeleteMarksTableRowQuery = $"DELETE FROM marks WHERE id = {row.Id}";
            new SQLiteCommand(sqlDeleteAboutTableRowQuery, connection).ExecuteNonQuery();
            new SQLiteCommand(sqlDeleteMarksTableRowQuery, connection).ExecuteNonQuery();

        }


        void UpdateDataGrid(ref DataGrid dataGrid, SQLiteDataReader reader) {
            dataGrid.Items.Clear();
            while (reader.Read()) {
                
                StudentRow dataGridRow = new StudentRow(int.Parse(reader["id"].ToString()), reader["fullname"].ToString(), reader["markMath"].ToString(), reader["markPhysics"].ToString());
                dataGridRow.IdInDB = int.Parse(reader["id"].ToString());
                dataGrid.Items.Add(dataGridRow);
            }
        }


        void InitializeStudentsDataBase(ref SQLiteConnection connection) {
            // creating tables in database
            // sqlite commands for tables
            string sqlCreateAboutTableQuery = "CREATE TABLE about (id INTEGER, fullname VARCHAR(30))"; // sql table with id and name
            string sqlCreateMarksTableQuery = "CREATE TABLE marks (id INTEGER, markMath VARCHAR(10), markPhysics VARCHAR(10))"; // sql table with id and marks(math, physics)
            // create tables execution
            new SQLiteCommand(sqlCreateAboutTableQuery, connection).ExecuteNonQuery();
            new SQLiteCommand(sqlCreateMarksTableQuery, connection).ExecuteNonQuery();

        }
        void ShowDBCreateDialogAndConnect() {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "Students";
            dialog.DefaultExt = ".sqlite";
            dialog.Filter = "SQLite Database (.sqlite)|*sqlite";

            Nullable<bool> result = dialog.ShowDialog();
            if (result == true) {
                SQLiteConnection.CreateFile(dialog.FileName);
                StudentsDBConnection = ConnectToDB(dialog.FileName);
                // set filename without path to title
                var splitedPath = dialog.FileName.Split('\\');
                Title = "File: " + splitedPath[splitedPath.Length - 1];

                InitializeStudentsDataBase(ref StudentsDBConnection);
            }
        }

        void ShowOpenDBDialogAndConnect() {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.DefaultExt = ".sqlite";
            dialog.Filter = "SQLite Database (.sqlite)|*sqlite";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true) {
                StudentsDBConnection = ConnectToDB(dialog.FileName);
                // set filename without path to title
                var splitedPath = dialog.FileName.Split('\\');
                Title = "File: " + splitedPath[splitedPath.Length - 1];

                UpdateDataGrid(ref DataBaseDataGrid, GetStudentsDataReader(ref StudentsDBConnection));
            }
        }
        private void Btn_CreateDB_Click(object sender, RoutedEventArgs e) {
            ShowDBCreateDialogAndConnect();
        }

        private void Btn_OpenDB_Click(object sender, RoutedEventArgs e) {
            ShowOpenDBDialogAndConnect();
        }

        private void Btn_AddStudent_Click(object sender, RoutedEventArgs e) {
            AddOrEditStudentWindow addWindow = new AddOrEditStudentWindow("add");
            Nullable<bool> result = addWindow.ShowDialog();
            if (result == true) {
                StudentRow addedRow = new StudentRow(addWindow.StudentId, addWindow.StudentFullname, addWindow.StudentMarkMath, addWindow.StudentMarkPhysics);
                DataBaseDataGrid.Items.Add(addedRow);
                UncreatedStudentsDataRows.Add(addedRow);
                ChangeWindowStyleToUnsaved();
                HasUnapplyedDataRows = true;
                
            }
        }

        private void Btn_EditSelected_Click(object sender, RoutedEventArgs e) {
            int selectedIndex = DataBaseDataGrid.SelectedIndex;
            if (selectedIndex != -1) { // if selected
                StudentRow pickedRow = (StudentRow)DataBaseDataGrid.SelectedItem;
                AddOrEditStudentWindow editWindow = new AddOrEditStudentWindow("edit",  pickedRow.Id, pickedRow.Fullname, pickedRow.MarkMath, pickedRow.MarkPhysics);
                Nullable<bool> result = editWindow.ShowDialog();
                if (result == true) {
                    StudentRow editedRow = new StudentRow(editWindow.StudentId, editWindow.StudentFullname, editWindow.StudentMarkMath, editWindow.StudentMarkPhysics);
                    if (editedRow.IdInDB == -1) {
                        editedRow.IdInDB = pickedRow.Id;
                    }
                    DataBaseDataGrid.Items[selectedIndex] = editedRow;
                    UnupdatedStudentsDataRows.Add(editedRow);
                    ChangeWindowStyleToUnsaved();
                    HasUnapplyedDataRows = true;
                }
            } else {
                MessageBox.Show("You must select DB field to edit it");
            }
        }

        private void Btn_SaveDB_Click(object sender, RoutedEventArgs e) {
            if (StudentsDBConnection != null) {
                ApplyChangesToDB(ref StudentsDBConnection);
            } else {
                ShowDBCreateDialogAndConnect();
                ApplyChangesToDB(ref StudentsDBConnection);
            }
            
        }

        private void Btn_RemoveSelected_Click(object sender, RoutedEventArgs e) {
            int selectedIndex = DataBaseDataGrid.SelectedIndex;
            if (selectedIndex != -1) { // if selected
                StudentRow pickedRow = (StudentRow)DataBaseDataGrid.SelectedItem;
                UndeletedStudentsDataRows.Add(pickedRow);
                DataBaseDataGrid.Items.RemoveAt(selectedIndex);
                if (DataBaseDataGrid.Columns.Count > 0) {
                    DataBaseDataGrid.SelectedIndex = 0;
                    ChangeWindowStyleToUnsaved();
                    HasUnapplyedDataRows = true;
                }
            }
        }
    }
}
