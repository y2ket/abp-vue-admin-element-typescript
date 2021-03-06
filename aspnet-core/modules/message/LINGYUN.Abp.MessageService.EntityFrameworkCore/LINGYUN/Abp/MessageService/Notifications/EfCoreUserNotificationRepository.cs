﻿using LINGYUN.Abp.MessageService.EntityFrameworkCore;
using LINGYUN.Abp.Notifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace LINGYUN.Abp.MessageService.Notifications
{
    public class EfCoreUserNotificationRepository : EfCoreRepository<MessageServiceDbContext, UserNotification, long>,
        IUserNotificationRepository, ITransientDependency
    {
        public EfCoreUserNotificationRepository(
            IDbContextProvider<MessageServiceDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<bool> AnyAsync(Guid userId, long notificationId)
        {
            return await DbSet
                .AnyAsync(x => x.NotificationId.Equals(notificationId) && x.UserId.Equals(userId));
        }

        public async Task InsertUserNotificationsAsync(IEnumerable<UserNotification> userNotifications)
        {
            await DbSet.AddRangeAsync(userNotifications);
        }

        public async Task ChangeUserNotificationReadStateAsync(Guid userId, long notificationId, NotificationReadState readState)
        {
            var userNofitication = await GetByIdAsync(userId, notificationId);
            userNofitication.ChangeReadState(readState);

            DbSet.Update(userNofitication);
        }

        public async Task<UserNotification> GetByIdAsync(Guid userId, long notificationId)
        {
            var userNofitication = await DbSet
                .Where(x => x.NotificationId.Equals(notificationId) && x.UserId.Equals(userId))
                .FirstOrDefaultAsync();

            return userNofitication;
        }

        public async Task<List<Notification>> GetNotificationsAsync(Guid userId, NotificationReadState readState = NotificationReadState.UnRead, int maxResultCount = 10)
        {

            var userNofitications = await (from un in DbContext.Set<UserNotification>()
                                     join n in DbContext.Set<Notification>()
                                         on un.NotificationId equals n.NotificationId
                                     where un.UserId.Equals(userId) && un.ReadStatus.Equals(readState)
                                     orderby n.NotificationId descending
                                           select n)
                                           .Distinct()
                                           .Take(maxResultCount)
                                           .ToListAsync();
            return userNofitications;
        }

        public virtual async Task<long> GetCountAsync(Guid userId, string filter = "", NotificationReadState readState = NotificationReadState.UnRead)
        {
            var userNofiticationCount = await (from un in DbContext.Set<UserNotification>()
                                           join n in DbContext.Set<Notification>()
                                               on un.NotificationId equals n.NotificationId
                                           where un.UserId.Equals(userId) && un.ReadStatus.Equals(readState)
                                           && (n.NotificationName.Contains(filter) || n.NotificationTypeName.Contains(filter)
                                                || n.NotificationCateGory.Contains(filter))
                                           select n)
                                              .Distinct()
                                              .LongCountAsync();
            return userNofiticationCount;
        }

        public virtual async Task<List<Notification>> GetNotificationsAsync(Guid userId, string filter = "", string sorting = "NotificationId", NotificationReadState readState = NotificationReadState.UnRead, int skipCount = 1, int maxResultCount = 10)
        {
            var userNofitications = await (from un in DbContext.Set<UserNotification>()
                                               join n in DbContext.Set<Notification>()
                                                   on un.NotificationId equals n.NotificationId
                                               where un.UserId.Equals(userId) && un.ReadStatus.Equals(readState)
                                               && (n.NotificationName.Contains(filter) || n.NotificationTypeName.Contains(filter)
                                                    || n.NotificationCateGory.Contains(filter))
                                               orderby sorting ?? nameof(Notification.NotificationId) descending
                                               select n)
                                              .Distinct()
                                              .Page(skipCount, maxResultCount)
                                              .AsNoTracking()
                                              .ToListAsync();
            return userNofitications;
        }
    }
}
