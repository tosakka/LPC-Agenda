using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Collections;
using System.Xml.Linq;

namespace Practica_Agenda
{

    public partial class Form1 : Form
    {
        private SqlConnection connection = new SqlConnection("Data Source=RIN;Initial Catalog=PracticaAgenda;Integrated Security=True");

        public Form1()
        {
            InitializeComponent();

        }

        private void txtNombre_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            connection.Open();
            consultar();
            connection.Close();
        }

        private void consultar()
        {
            
            SqlCommand command = new SqlCommand("SELECT c.id, c.Nombre, c.Apellido, c.FechaNacimiento, t.Numero as 'Numero de Telefono', CASE WHEN f.id IS NULL THEN 'No' ELSE 'Si' END as 'Favorito' FROM Contacto c LEFT JOIN Telefono t ON c.id = t.contacto_id LEFT JOIN Favorito f ON c.id = f.contacto_id", connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            dataGridView1.DataSource = table;
            
        }


        private void btnAgregar_Click(object sender, EventArgs e)
        {
            connection.Open();
            Agregar();
            connection.Close();
        }

        private void Agregar()
        {
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            string fechaNacimiento = dateFeNa.Value.ToString("yyyy-MM-dd");
            string numeroTelefono = txtNum.Text;

            string consultaInsertarContacto = "INSERT INTO Contacto (nombre, apellido, fechaNacimiento) VALUES (@nombre, @apellido, @fechaNacimiento)";
            using (SqlCommand cmd = new SqlCommand(consultaInsertarContacto, connection))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                cmd.Parameters.AddWithValue("@fechaNacimiento", fechaNacimiento);
                cmd.ExecuteNonQuery();
            }

            int contactoId = 0;
            string consultaObtenerId = "SELECT id FROM Contacto WHERE nombre=@nombre AND apellido=@apellido";
            using (SqlCommand cmd = new SqlCommand(consultaObtenerId, connection))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    contactoId = reader.GetInt32(0);
                }
                reader.Close();
            }

            string consultaInsertarTelefono = "INSERT INTO Telefono (contacto_id, Numero) VALUES (@contacto_id, @Numero)";
            using (SqlCommand cmd = new SqlCommand(consultaInsertarTelefono, connection))
            {
                cmd.Parameters.AddWithValue("@contacto_id", contactoId);
                cmd.Parameters.AddWithValue("@Numero", numeroTelefono);
                cmd.ExecuteNonQuery();
            }

            if (ckFav.Checked)
            {
                string consultaInsertarFavorito = "INSERT INTO Favorito (contacto_id) VALUES (@contacto_id)";
                using (SqlCommand cmd = new SqlCommand(consultaInsertarFavorito, connection))
                {
                    cmd.Parameters.AddWithValue("@contacto_id", contactoId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            DateTime fechaNacimiento = dateFeNa.Value;
            string numeroTelefono = txtNum.Text;
            bool esFavorito = ckFav.Checked;

            connection.Open();
            Editar(nombre, apellido, fechaNacimiento, numeroTelefono, esFavorito);
            connection.Close();
        }

        private void Editar(string nombre, string apellido, DateTime fechaNacimiento, string numeroTelefono, bool esFavorito)
        {
            int id = 0;
            string consultaObtenerId = "SELECT Contacto.id FROM Contacto INNER JOIN Telefono ON Contacto.id = Telefono.contacto_id WHERE Contacto.nombre = @nombre AND Telefono.Numero = @Numero";
            using (SqlCommand cmd = new SqlCommand(consultaObtenerId, connection))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@Numero", numeroTelefono);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
                reader.Close();
            }

            if (id > 0)
            {

                string consultaActualizarContacto = "UPDATE Contacto SET nombre = @nombre, apellido = @apellido, fechaNacimiento = @fechaNacimiento WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(consultaActualizarContacto, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@fechaNacimiento", fechaNacimiento.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                string consultaActualizarTelefono = "UPDATE Telefono SET Numero = @Numero WHERE contacto_id = @contacto_id";
                using (SqlCommand cmd = new SqlCommand(consultaActualizarTelefono, connection))
                {
                    cmd.Parameters.AddWithValue("@Numero", numeroTelefono);
                    cmd.Parameters.AddWithValue("@contacto_id", id);
                    cmd.ExecuteNonQuery();
                }

                if (esFavorito)
                {

                    bool yaEsFavorito = false;
                    string consultaVerificarFavorito = "SELECT COUNT(*) FROM Favorito WHERE contacto_id = @contacto_id";
                    using (SqlCommand cmd = new SqlCommand(consultaVerificarFavorito, connection))
                    {
                        cmd.Parameters.AddWithValue("@contacto_id", id);
                        yaEsFavorito = ((int)cmd.ExecuteScalar()) > 0;
                    }

                    if (!yaEsFavorito)
                    {
                        string consultaInsertarFavorito = "INSERT INTO Favorito (contacto_id) VALUES (@contacto_id)";
                        using (SqlCommand cmd = new SqlCommand(consultaInsertarFavorito, connection))
                        {
                            cmd.Parameters.AddWithValue("@contacto_id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    string consultaEliminarFavorito = "DELETE FROM Favorito WHERE contacto_id = @contacto_id";
                    using (SqlCommand cmd = new SqlCommand(consultaEliminarFavorito, connection))
                    {
                        cmd.Parameters.AddWithValue("@contacto_id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;

            connection.Open();
            Borrar(nombre, apellido);
            connection.Close();
        }

        private void Borrar(string nombre, string apellido)
        {
            int id = 0;
            string consultaObtenerId = "SELECT id FROM Contacto WHERE nombre = @nombre AND apellido = @apellido";
            using (SqlCommand cmd = new SqlCommand(consultaObtenerId, connection))
            {
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
                reader.Close();
            }

            if (id > 0)
            {
                string consultaEliminarFavorito = "DELETE FROM Favorito WHERE contacto_id = @contacto_id";
                using (SqlCommand cmd = new SqlCommand(consultaEliminarFavorito, connection))
                {
                    cmd.Parameters.AddWithValue("@contacto_id", id);
                    cmd.ExecuteNonQuery();
                }

                string consultaEliminarTelefono = "DELETE FROM Telefono WHERE contacto_id = @contacto_id";
                using (SqlCommand cmd = new SqlCommand(consultaEliminarTelefono, connection))
                {
                    cmd.Parameters.AddWithValue("@contacto_id", id);
                    cmd.ExecuteNonQuery();
                }

                string consultaEliminarContacto = "DELETE FROM Contacto WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(consultaEliminarContacto, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void ckFav_CheckedChanged(object sender, EventArgs e)
        {

        }


    }
}