using System.Threading.Tasks;
using HeroQuest.Domain;

namespace HeroQuest.Systems.Dungeon
{
    /// <summary>
    /// 副本资源服务接口。
    /// 所有方法为异步（返回 Task），调用方需 await。
    /// </summary>
    public interface IDungeonResourceService
    {
        /// <summary>
        /// 获取资源节点。
        /// 无单节点查询协议：资源节点随副本进入响应（EnterDungeonResponse.resources）下发，本地返回空节点。
        /// </summary>
        /// <param name="nodeId">资源节点 ID（字符串形式的 ulong，对应协议 resource_id）</param>
        Task<ServiceResult<DungeonResourceNode>> GetNode(string nodeId);

        /// <summary>
        /// 采集资源节点。
        /// </summary>
        /// <param name="nodeId">资源节点 ID（字符串形式的 ulong，对应协议 resource_id）</param>
        Task<ServiceResult> Collect(string nodeId);
    }
}
