

## Codely Structured Memories

### User

### Feedback
- [2026-07-27 22:48:28] 前后端联调验证通过 (2026-07-27): 登录→选角→进副本→怪物同步→战斗交互 全链路 OK。**Why:** 阶段二拆解巨型类时 NetworkEventHandler 更新 GameplayState 后未回写 PrototypeGameplayFlow 本地字段，导致 visibleMonsters/currentLayer 为空。**How to apply:** 修改 PrototypeGameplayFlow 状态相关字段时，确保 SyncStateFromGameplayState() 覆盖回写。

### Project
- [2026-07-27 22:48:28] Go 后端位于 C:\Users\aqi\Desktop\2d-rpg\rpg-go (非 rpg-go-main)，Go 1.26.2 已安装，docker-compose 可用。服务器远程地址 ws://129.226.195.114:8080/ws 已部署运行，JWT 密钥 hero-quest-secret-key 客户端服务端共享。
### Reference

