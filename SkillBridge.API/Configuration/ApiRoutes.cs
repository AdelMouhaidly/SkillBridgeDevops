namespace SkillBridge.API.Configuration;

public static class ApiRoutes
{
    public static class Usuarios
    {
        public const string GetAll = "GetUsuariosV1";
        public const string GetById = "GetUsuarioByIdV1";
        public const string Create = "CreateUsuarioV1";
        public const string Update = "UpdateUsuarioV1";
        public const string Delete = "DeleteUsuarioV1";
        public const string Summary = "GetUsuariosResumoV2";
    }

    public static class Vagas
    {
        public const string GetAll = "GetVagasV1";
        public const string GetById = "GetVagaByIdV1";
        public const string Create = "CreateVagaV1";
        public const string Update = "UpdateVagaV1";
        public const string Delete = "DeleteVagaV1";
    }

    public static class Aplicacoes
    {
        public const string GetAll = "GetAplicacoesV1";
        public const string GetById = "GetAplicacaoByIdV1";
        public const string GetByUsuario = "GetAplicacoesPorUsuarioV1";
        public const string GetByVaga = "GetAplicacoesPorVagaV1";
        public const string Create = "CreateAplicacaoV1";
        public const string Update = "UpdateAplicacaoV1";
        public const string Delete = "DeleteAplicacaoV1";
    }

    public static class Match
    {
        public const string Calculate = "CalculateMatchV1";
        public const string CalculateByAplicacao = "CalculateMatchByAplicacaoV1";
    }
}
