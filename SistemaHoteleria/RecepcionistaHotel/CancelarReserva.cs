﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SistemaHoteleria.Datos;

namespace SistemaHoteleria.RecepcionistaHotel
{
    public partial class CancelarReserva : Form
    {
        public CancelarReserva()
        {
            InitializeComponent();
        }

        public void inicializar()
        {
            dtpFechaIngreso.Value = DateTime.Now.Date;
            dtpFechaSalida.Value = DateTime.Now.Date.AddDays(1);
            txtFiltroId.text = "ID...";
            lblIdReserva.Text = "0";
        }

        public void consultarReservas()
        {
            
            using (SistemaHotelWaraEntitiesV1 DB = new SistemaHotelWaraEntitiesV1())
            {
                try
                {

                    if (lblIdReserva.Text != "")
                    {
                        if (txtFiltroId.text == "TODOS")
                        {
                            dgvReservas.DataSource = (from dr in DB.DetalleReservas
                                                      join res in DB.Reservas on dr.idDetalleReservas equals res.idReserva
                                                      join f in DB.Factura on dr.idDetalleReservas equals f.idFactura
                                                      join r in DB.Recepcionista on dr.idEmpleado equals r.idEmpleado
                                                      join h in DB.Habitaciones on dr.idHabitacion equals h.idHabitacion
                                                      join dh in DB.DetalleHabitacion on h.idHabitacion equals dh.idDetalleHabitacion
                                                      where f.fecha.Value >= dtpFechaIngreso.Value && f.fecha.Value <= dtpFechaSalida.Value && res.estado == "SIN PAGAR"
                                                      orderby res.idReserva descending
                                                      select new { ID_RESERVA = dr.idDetalleReservas, RECEPCIONISTA = (r.nombre + " " + r.paterno + " " + r.materno), ID_HABITACION = h.idHabitacion, PISO = h.nroPiso, CAMAS = h.nroCamas, TIPO = dh.idTipo }
                                                                  ).ToList();
                        }
                        else
                        {
                            int i = int.Parse(txtFiltroId.text);
                            dgvReservas.DataSource = (from dr in DB.DetalleReservas
                                                      join res in DB.Reservas on dr.idDetalleReservas equals res.idReserva
                                                      join f in DB.Factura on dr.idDetalleReservas equals f.idFactura
                                                      join r in DB.Recepcionista on dr.idEmpleado equals r.idEmpleado
                                                      join h in DB.Habitaciones on dr.idHabitacion equals h.idHabitacion
                                                      join dh in DB.DetalleHabitacion on h.idHabitacion equals dh.idDetalleHabitacion
                                                      where f.fecha.Value >= dtpFechaIngreso.Value && f.fecha.Value <= dtpFechaSalida.Value && res.estado == "SIN PAGAR" && res.idReserva == i
                                                      orderby res.idReserva descending
                                                      select new { ID_RESERVA = dr.idDetalleReservas, RECEPCIONISTA = (r.nombre + " " + r.paterno + " " + r.materno), ID_HABITACION = h.idHabitacion, PISO = h.nroPiso, CAMAS = h.nroCamas, TIPO = dh.idTipo }
                                                                  ).ToList();
                        }
                        
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void VerReservas_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'sistemaHotelWaraDataSet.Tipo' Puede moverla o quitarla según sea necesario.
            this.tipoTableAdapter.Fill(this.sistemaHotelWaraDataSet.Tipo);
            inicializar();
        }

        private void dtpFechaIngreso_onValueChanged(object sender, EventArgs e)
        {
            consultarReservas();
        }

        private void dtpFechaSalida_onValueChanged(object sender, EventArgs e)
        {
            consultarReservas();
        }

        private void txtFiltroId_OnTextChange(object sender, EventArgs e)
        {
            consultarReservas();
        }

        private void txtFiltroApellido_OnTextChange(object sender, EventArgs e)
        {
            consultarReservas();
        }

        private void cbFiltroTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFiltroId.text = "ID...";
            consultarReservas();
        }

        private void dgvReservas_Click(object sender, EventArgs e)
        {
            try
            {
                int idReserva = int.Parse(dgvReservas.Rows[dgvReservas.CurrentRow.Index].Cells[0].Value.ToString());
                lblIdReserva.Text = idReserva.ToString();
                string idHabitacion = dgvReservas.Rows[dgvReservas.CurrentRow.Index].Cells[2].Value.ToString();

                CargarServicios(idHabitacion);

                List<DateTime> fechas = new List<DateTime>();

                using (SistemaHotelWaraEntitiesV1 DB = new SistemaHotelWaraEntitiesV1())
                {
                    var habitaciones = (from r in DB.Reservas
                                        where (r.estado != "CANCELADO") && r.idReserva == idReserva
                                        select new {r.ingreso, r.salida}
                                                          ).ToList();
                    foreach (var h in habitaciones)
                    {
                        for (DateTime i = (DateTime)h.ingreso; i <= (DateTime)h.salida; i = i.AddDays(1))
                        {
                            fechas.Add(i);
                        }
                    }
                }

                calendarEstadoHabitacion.BoldedDates = fechas.ToArray();

                using (SistemaHotelWaraEntitiesV1 DB = new SistemaHotelWaraEntitiesV1())
                {
                    dgvHuespedes.DataSource = (from r in DB.RegistroHuespedes
                                               join hu in DB.Huespedes on r.idHuespedes equals hu.idHuesped
                                               where r.idReserva == idReserva
                                               select new {hu.idHuesped, hu.nombre, hu.paterno, hu.materno}
                                                          ).ToList();
                }
            }
            catch (Exception)
            {
            }
        }

        public void CargarServicios(string idHabitacion)
        {
            using (SistemaHotelWaraEntitiesV1 DB = new SistemaHotelWaraEntitiesV1())
            {
                Servicios s = DB.Servicios.Find(idHabitacion);
                swAireAcondicionado.Value = (bool)s.aireAcondicionado;
                swAlberca.Value = (bool)s.alberca;
                swBarHotel.Value = (bool)s.barHotel;
                swEstacionamiento.Value = (bool)s.estacionamiento;
                swGym.Value = (bool)s.gym;
                swMascotas.Value = (bool)s.mascotas;
                swRestaurante.Value = (bool)s.restaurante;
                swSpa.Value = (bool)s.spa;
                swWifiHabitacion.Value = (bool)s.wifiHabitacion;
                swWifiLobby.Value = (bool)s.wifiLobby;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Esta segura que desea cancelar la reserva?", "Mensaje de Confirmacion", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                using (SistemaHotelWaraEntitiesV1 DB = new SistemaHotelWaraEntitiesV1())
                {
                    int idReservaInt = int.Parse(dgvReservas.Rows[dgvReservas.CurrentRow.Index].Cells[0].Value.ToString());
                    Reservas nuevo = DB.Reservas.Find(idReservaInt);
                    nuevo.estado = "CANCELADO";
                    DB.Entry(nuevo).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                }
                consultarReservas();
            }
        }
    }
}
