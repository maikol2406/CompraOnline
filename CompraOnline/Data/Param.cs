namespace CompraOnline.Data
{
    public class Param
    {
        public Parametros parametrosDB()
        {
            Parametros parametro = new Parametros();
            parametro.DataSource = "mate.superbaterias.com";
            parametro.UserID = "sa";
            parametro.Password = "5up3r\\B@t3r145";
            parametro.InitialCatalog = "CompraOnline";

            return parametro;
        }
    }
}
