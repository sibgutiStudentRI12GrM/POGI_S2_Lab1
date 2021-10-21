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
using System.Windows.Shapes;

namespace lab1 {
    
    public partial class AddOrEditStudentWindow : Window {
        string WindowType;
        public int StudentId;
        public string StudentFullname;
        public string StudentMarkMath;
        public string StudentMarkPhysics;
        public AddOrEditStudentWindow(string windowType, int studentId = 0, string studentFullname = "", string markMath = "", string markPhysics = "") {
            InitializeComponent();
            // validate window type is add or edit
            if (windowType == "add") {
                Title = "Add student";
                Btn_Submit.Content = "Add student";
            } else if (windowType == "edit") {
                Title = "Edit student";
                Btn_Submit.Content = "Edit student";

                StudentId = studentId;
                StudentFullname = studentFullname;
                StudentMarkMath = markMath;
                StudentMarkPhysics = markPhysics;

                TextBoxId.Text = StudentId.ToString();
                TextBoxFullname.Text = StudentFullname;
                TextBoxMarkMath.Text = StudentMarkMath;
                TextBoxMarkPhysics.Text = StudentMarkPhysics;
            } else {
                throw new Exception("Incorrect addEditDialogType");
            }
            WindowType = windowType;
        }

        private void Btn_Submit_Click(object sender, RoutedEventArgs e) {
            if (TextBoxId.Text != "" && !Int32.TryParse(TextBoxId.Text, out _)) {
                MessageBox.Show("Id must be decimal number");
            }

            if (TextBoxId.Text != "" && TextBoxFullname.Text != "" && TextBoxMarkMath.Text != "" && TextBoxMarkPhysics.Text != "") {
                StudentId = int.Parse(TextBoxId.Text);
                StudentFullname = TextBoxFullname.Text;
                StudentMarkMath = TextBoxMarkMath.Text;
                StudentMarkPhysics = TextBoxMarkPhysics.Text;
                DialogResult = true;
                return;
            }
            
            MessageBox.Show("You must fill all fields");


        }
    }
}
