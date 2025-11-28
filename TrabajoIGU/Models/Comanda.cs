using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TrabajoIGU.Models
{
    public class Comanda : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int IdMesa { get; set; }
        public DateTime Fecha { get; set; }

        private Dictionary<Plato, int> platos;
        public Dictionary<Plato, int> Platos
        {
            get => platos;
            set
            {
                if (!ReferenceEquals(platos, value))
                {
                    platos = value;
                    OnPropertyChanged(nameof(Platos));
                }
            }
        }

        public Comanda(int idMesa)
        {
            IdMesa = idMesa;
            Fecha = DateTime.Now;
            Platos = new Dictionary<Plato, int>();
        }

        public int TotalPlatos()
        {
            int total = 0;
            foreach (var p in Platos.Values)
                total += p;
            return total;
        }

        public void AñadirPlato(Plato plato)
        {
            if (Platos.ContainsKey(plato))
                Platos[plato]++;
            else
                Platos[plato] = 1;

            OnPropertyChanged(nameof(Platos));
        }

        public void QuitarPlato(Plato plato)
        {
            if (Platos.ContainsKey(plato))
            {
                Platos[plato]--;
                if (Platos[plato] <= 0)
                    Platos.Remove(plato);
                OnPropertyChanged(nameof(Platos));
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
