# HmDianPing (黑马点评 .NET 版)

**HmDianPing** 是一个基于 **.NET 10** 和 **Blazor Server** 构建的本地生活信息点评平台。该项目模仿了大众点评的核心功能，集成了 MySQL 持久化存储与 Redis 缓存，旨在提供高性能的店铺管理与搜索体验。

## ✨ 核心特性

* **店铺管理**: 支持店铺数据的增删改查（CRUD）。
* **搜索与发现**: 提供基于名称或商圈的店铺搜索功能。
* **分页浏览**: 支持大量店铺数据的分页展示与导航。
* **交互式 UI**: 使用 Blazor Interactive Server 模式，提供类似 SPA 的流畅用户体验。
* **数据缓存**: 集成 Redis 进行连接复用与数据缓存，提升系统性能。
* **安全性**: 集成 JWT 认证与 BCrypt 密码加密。

## 🛠️ 技术栈

* **框架**: .NET 10.0, ASP.NET Core Blazor Server
* **数据库**: MySQL 8.x (使用 Pomelo.EntityFrameworkCore.MySql)
* **缓存**: Redis (StackExchange.Redis)
* **ORM**: Entity Framework Core 9.0
* **日志**: Serilog
* **对象映射**: AutoMapper

## 🚀 快速开始

### 1. 环境准备
确保您的开发环境已安装：
* .NET 10.0 SDK (或兼容版本)
* MySQL Server
* Redis Server

### 2. 配置数据库与缓存
修改 `HmDianPing/appsettings.json` 文件，配置您的数据库和 Redis 连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=hmdianping_db;user=root;password=your_password;",
    "Redis": "your_redis_host:6379"
  }
}
