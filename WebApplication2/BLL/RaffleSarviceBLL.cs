using WebApplication2.DAL;
using WebApplication2.Models;
using WebApplication2.Models.DTO;

namespace WebApplication2.BLL
{
    public class RaffleSarviceBLL : IRaffleBLL
    {
        private readonly IWinnerDAL _winnerDal;
        private readonly IOrderDal _orderDal;
        private readonly IUserDal _userDal;
        private readonly IGiftDal _giftDal;
        private readonly IEmailService _emailService;
        private readonly ILogger<RaffleSarviceBLL> _logger;

        public RaffleSarviceBLL(
            IWinnerDAL winnerDal,
            IOrderDal orderDal,
            IUserDal userDal,
            IGiftDal giftDal,
            IEmailService emailService,
            ILogger<RaffleSarviceBLL> logger)
        {
            _winnerDal = winnerDal ?? throw new ArgumentNullException(nameof(winnerDal));
            _orderDal = orderDal ?? throw new ArgumentNullException(nameof(orderDal));
            _userDal = userDal ?? throw new ArgumentNullException(nameof(userDal));
            _giftDal = giftDal ?? throw new ArgumentNullException(nameof(giftDal));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<WinnerModel> RunRaffle(int giftId)
        {
            _logger.LogInformation("התחילה הגרלה עבור מתנה {GiftId}", giftId);

            // Check if gift already has a winner
            if (await _winnerDal.IsGiftAlreadyWonAsync(giftId))
            {
                _logger.LogWarning("מתנה {GiftId} כבר הוגרלה", giftId);
                throw new BusinessException("מתנה זו כבר הוגרלה ויש לה זוכה");
            }

            // Get raffle pool from DAL
            var rafflePoolData = await _orderDal.GetRafflePoolByGiftIdAsync(giftId);

            if (!rafflePoolData.Any())
            {
                _logger.LogWarning("אין כרטיסים למתנה {GiftId} - ההגרלה בוטלה", giftId);
                return null;
            }

            _logger.LogInformation("נמצאו {TicketCount} כרטיסים עבור מתנה {GiftId}", 
                rafflePoolData.Sum(x => x.Quantity), giftId);

            // Build raffle pool
            int winnerUserId = SelectRandomWinner(rafflePoolData);

            _logger.LogInformation("בחור זוכה: UserId={WinnerUserId}", winnerUserId);

            // Create and save winner
            var winner = new WinnerModel
            {
                GiftId = giftId,
                UserId = winnerUserId
            };

            try
            {
                await _winnerDal.AddWinner(winner);
                _logger.LogInformation(
                    "הזוכה נשמר בהצלחה - GiftId={GiftId}, UserId={UserId}", 
                    giftId, winnerUserId);

                // Retrieve winner with full details
                winner = await _winnerDal.GetWinnerByGiftIdAsync(giftId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "שגיאה בשמירת הזוכה - GiftId={GiftId}, UserId={UserId}", 
                    giftId, winnerUserId);
                throw;
            }

            // Send notification email
            await SendWinnerNotificationAsync(winner);

            _logger.LogInformation("הגרלה הושלמה בהצלחה - Winner UserId={UserId}", winnerUserId);
            return winner;
        }

        /// <summary>
        /// Select random winner from raffle pool
        /// </summary>
        private int SelectRandomWinner(List<(int UserId, int Quantity)> rafflePool)
        {
            // Build pool with duplicates based on ticket quantity
            var expandedPool = new List<int>(rafflePool.Sum(x => x.Quantity));
            foreach (var (userId, quantity) in rafflePool)
            {
                expandedPool.AddRange(Enumerable.Repeat(userId, quantity));
            }

            // Select random winner
            var random = new Random();
            int winnerIndex = random.Next(expandedPool.Count);
            return expandedPool[winnerIndex];
        }

        /// <summary>
        /// Send winner notification email
        /// </summary>
        private async Task SendWinnerNotificationAsync(WinnerModel winner)
        {
            if (winner?.User == null || winner?.Gift == null)
            {
                _logger.LogWarning(
                    "לא ניתן לשלוח מייל - User או Gift חסר. WinnerId={WinnerId}", 
                    winner?.Id);
                return;
            }

            try
            {
                await _emailService.SendWinnerNotificationAsync(
                    winner.User.Email, 
                    winner.User.Name, 
                    winner.Gift.Name);
                
                _logger.LogInformation(
                    "מייל זכייה נשלח בהצלחה - Email={Email}, Gift={GiftName}", 
                    winner.User.Email, 
                    winner.Gift.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "שגיאה בשליחת מייל זכייה ל-UserId={UserId}", 
                    winner.UserId);
                // Don't throw - email failure shouldn't fail the entire raffle
            }
        }
    }
}
