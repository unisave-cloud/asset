using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    /*
     * Co teda jako dál?
     * TODO: ========================
     * - načítání vlastníků vyřeš na LoadEntity dotazu
     * - takže na něj napiš testy a pak pořeš dotazování
     * - pro dotazování entit se to pak bude chovat stejně
     * - ještě si na dotazování hráčů můžeš rovnou vyzkoušet block fetch cursor
     *
     * TODO: THIS:
     * - Čili ÚPLNĚ NEJDŘÍV vlastně naimplementuj nějaký dotaz, co dokáže
     *     iterovat přes vlastníky nějaké entity
     *     (pokud entita neexistuje, iteruju prázný pole, což udělá už datbáze)
     */
    
    
    /*
     * - Add owner to an entity
     * - Remove owner
     * - Query owners
     *
     * - Load entity by id
     * - Cannot load entity with given id from different DB
     * - Missing entity returns null
     * - Owners for an entity get loaded sometimes
     *
     * - Update entity tests
     *
     * - transactions & locking
     */
}