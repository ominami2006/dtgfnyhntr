using System;
using System.Data;
using System.Windows.Forms;

namespace bookservice
{
    public partial class UsersManagerForm : Form
    {
        private UsersManagerSQL usersManagerSql;
        private User currentUser; // Администратор, открывший форму
        private DataTable usersDataTable;

        public UsersManagerForm(User adminUser)
        {
            InitializeComponent();
            this.currentUser = adminUser;
            this.usersManagerSql = new UsersManagerSQL();
        }

        private void UsersManagerForm_Load(object sender, EventArgs e)
        {
            LoadUsersData();
        }

        private void LoadUsersData()
        {
            usersDataTable = usersManagerSql.GetAllUsers();
            if (usersDataTable != null)
            {
                usersDataGridView.DataSource = usersDataTable;
                ConfigureDataGridView();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить данные пользователей.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Возможно, закрыть форму или отключить элементы управления
            }
        }

        private void ConfigureDataGridView()
        {
            if (usersDataGridView.Columns["id"] != null)
            {
                usersDataGridView.Columns["id"].HeaderText = "ID";
                usersDataGridView.Columns["id"].ReadOnly = true; // ID не должен редактироваться напрямую
            }
            if (usersDataGridView.Columns["login"] != null)
            {
                usersDataGridView.Columns["login"].HeaderText = "Логин";
            }
            if (usersDataGridView.Columns["is_admin"] != null)
            {
                usersDataGridView.Columns["is_admin"].HeaderText = "Администратор";
                // Для булевого поля DataGridViewCheckBoxColumn используется по умолчанию, что удобно
            }
            // usersDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Растянуть колонки
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Завершить редактирование текущей ячейки, чтобы изменения попали в DataTable
                this.Validate();
                usersDataGridView.EndEdit();

                // DataTable, привязанный к DataSource, уже содержит изменения
                DataTable changes = ((DataTable)usersDataGridView.DataSource).GetChanges();

                if (changes == null)
                {
                    MessageBox.Show("Нет изменений для сохранения.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (usersManagerSql.SaveChanges((DataTable)usersDataGridView.DataSource)) // Передаем всю таблицу, SqlDataAdapter разберется
                {
                    MessageBox.Show("Изменения успешно сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Обновляем DataTable, чтобы он отражал состояние после сохранения (например, AcceptChanges)
                    ((DataTable)usersDataGridView.DataSource).AcceptChanges();
                    // Или перезагружаем данные полностью, если есть авто-инкременты или триггеры на сервере
                    // LoadUsersData(); 
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить изменения. См. консоль для деталей или предыдущие сообщения.", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Можно предложить пользователю отменить изменения или попробовать снова
                    // ((DataTable)usersDataGridView.DataSource).RejectChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}