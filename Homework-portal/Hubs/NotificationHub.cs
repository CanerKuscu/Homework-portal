// ---- YENİ DOSYA OLUŞTUR ----
// Dosya Yolu: Homework-portal/Hubs/NotificationHub.cs

using Microsoft.AspNetCore.SignalR;

namespace Homework_portal.Hubs
{
    // Bu sınıf, SignalR'ın anlık bildirimler için
    // bağlantı merkezi (Hub) olarak çalışacak.
    public class NotificationHub : Hub
    {
        // Basit bir bildirim (broadcast) için
        // şimdilik buraya ekstra bir metot yazmamıza gerek yok.
        // İstemciler (tarayıcılar) bu Hub'a bağlanacak
        // ve biz Controller'dan bu Hub'a mesaj göndereceğiz.
    }
}