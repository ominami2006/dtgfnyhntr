using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace bookservice
{
    public class UsersManagerSQL : IDisposable
    {
        private Connection dbConnection;
        private SqlDataAdapter dataAdapter;
        private DataTable usersTable;

        public UsersManagerSQL()
        {
            dbConnection = new Connection();
            // Используем строку подключения для администратора
            // Убедитесь, что у пользователя 'admin' есть права на SELECT, INSERT, UPDATE, DELETE для таблицы users
            string adminConnectionString = "Data Source=localhost;Initial Catalog=bookservice;User ID=admin;Password=admin;";
            adminConnectionString = "Server=adclg1;Database=bookservice;Trusted_Connection=True;";
            dbConnection.SetСonnectionString(adminConnectionString);
        }

        public DataTable GetAllUsers()
        {
            try
            {
                dbConnection.OpenConnection();
                // Выбираем только id, login и is_admin. Пароли не должны загружаться для прямого редактирования.
                string query = "SELECT id, login, is_admin FROM users;";
                dataAdapter = new SqlDataAdapter(query, dbConnection.GetConnection());

                // SqlCommandBuilder автоматически генерирует команды INSERT, UPDATE, DELETE
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);

                usersTable = new DataTable();
                dataAdapter.Fill(usersTable);
                return usersTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке пользователей: {ex.Message}");
                // В реальном приложении здесь может быть более сложная обработка ошибок
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                dbConnection.CloseConnection();
            }
        }

        public bool SaveChanges(DataTable tableWithChanges)
        {
            if (dataAdapter == null || tableWithChanges == null)
            {
                return false;
            }

            try
            {
                dbConnection.OpenConnection();
                // Обновляем исходную usersTable изменениями из DataGridView, если они не были синхронизированы
                // usersTable должен быть тем же объектом DataTable, который привязан к DataGridView
                // и который был изначально заполнен через dataAdapter.Fill()
                int rowsAffected = dataAdapter.Update(tableWithChanges);
                return rowsAffected >= 0; // Update возвращает количество обновленных строк, или -1 при ошибке в некоторых случаях
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении изменений пользователей: {ex.Message}");
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                dbConnection.CloseConnection();
            }
        }

        public void Dispose()
        {
            if (dbConnection != null)
            {
                dbConnection.Dispose();
                dbConnection = null;
            }
            if (dataAdapter != null)
            {
                dataAdapter.Dispose();
                dataAdapter = null;
            }
            if (usersTable != null)
            {
                usersTable.Dispose();
                usersTable = null;
            }
            GC.SuppressFinalize(this);
        }

        ~UsersManagerSQL()
        {
            Dispose();
        }
    }
}