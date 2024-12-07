namespace CompraOnline.Data
{
    public class Param
    {
        public Parametros parametrosDB()
        {
            Parametros parametro = new Parametros();
            parametro.DataSource = "tcp:maikolacuna.database.windows.net,1433";
            parametro.UserID = "maikol2406";
            parametro.Password = "$Andres00*$";
            parametro.InitialCatalog = "CompraOnline";

            return parametro;
        }
    }
}
