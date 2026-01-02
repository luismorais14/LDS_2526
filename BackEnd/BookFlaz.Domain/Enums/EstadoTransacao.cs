using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Enums
{
    public enum EstadoTransacao
    {
        ACEITE,
        DEVOLUCAO_PENDENTE,
        PENDENTE,
        CONCLUIDA,
        CANCELADA
    }
}
