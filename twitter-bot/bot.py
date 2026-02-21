"""
Corplynk Twitter Bot
Gunde 2 tweet. Gun basinda rastgele 2 zaman secer (saat+dakika), vakti gelince atar.
Yari kalmissa kaldigindan devam eder.
"""

import json
import os
import sys
import random
import logging
from datetime import datetime, timezone
from pathlib import Path

import tweepy
from dotenv import load_dotenv

BASE_DIR = Path(__file__).parent
load_dotenv(BASE_DIR / ".env")

logging.basicConfig(
    filename=BASE_DIR / "bot.log",
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S"
)
logger = logging.getLogger(__name__)
console = logging.StreamHandler()
console.setLevel(logging.INFO)
console.setFormatter(logging.Formatter("%(asctime)s [%(levelname)s] %(message)s"))
logger.addHandler(console)

STATE_FILE = BASE_DIR / "tweets_posted.json"
TWEETS_FILE = BASE_DIR / "tweets.json"

# TR is saatleri (UTC+3). Bot TR saatiyle dusunur.
BUSINESS_START = 10  # TR 10:00
BUSINESS_END = 20    # TR 20:00
TWEETS_PER_DAY = 2
MIN_GAP_HOURS = 3    # 2 tweet arasi en az 3 saat


def load_state():
    if STATE_FILE.exists():
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"posted_ids": [], "today": None, "plan": []}


def save_state(state):
    with open(STATE_FILE, "w", encoding="utf-8") as f:
        json.dump(state, f, indent=2, ensure_ascii=False)


def load_tweets():
    with open(TWEETS_FILE, "r", encoding="utf-8") as f:
        return json.load(f)


def get_next_tweet(tweets, state):
    posted = set(state["posted_ids"])
    for t in tweets:
        if t["id"] not in posted:
            return t
    return None


def post_tweet(text):
    client = tweepy.Client(
        consumer_key=os.getenv("API_KEY"),
        consumer_secret=os.getenv("API_SECRET"),
        access_token=os.getenv("ACCESS_TOKEN"),
        access_token_secret=os.getenv("ACCESS_TOKEN_SECRET")
    )
    return client.create_tweet(text=text)


def make_daily_plan(tr_hour_now, tr_minute_now=0):
    """Bugunun tweet saatlerini rastgele sec (saat + dakika)."""
    # Dakika cinsinden calis (orn: 10:00 = 600, 19:59 = 1199)
    now_minutes = tr_hour_now * 60 + tr_minute_now
    start_minutes = max(BUSINESS_START * 60, now_minutes + 5)  # en erken 5dk sonra
    end_minutes = BUSINESS_END * 60  # 20:00 = 1200

    if start_minutes >= end_minutes:
        return []

    gap_minutes = MIN_GAP_HOURS * 60  # 3 saat = 180dk

    # 2 rastgele zaman sec, aralarinda en az gap_minutes olsun
    for _ in range(200):
        t1 = random.randint(start_minutes, end_minutes - 1)
        t2 = random.randint(start_minutes, end_minutes - 1)
        picks = sorted([t1, t2])
        if picks[1] - picks[0] >= gap_minutes:
            return picks

    # Bulamazsa basit dagitim
    mid = (start_minutes + end_minutes) // 2
    return [random.randint(start_minutes, mid - gap_minutes // 2),
            random.randint(mid + gap_minutes // 2, end_minutes - 1)]


def main():
    now_utc = datetime.now(timezone.utc)
    tr_hour = (now_utc.hour + 3) % 24
    tr_minute = now_utc.minute
    today = now_utc.strftime("%Y-%m-%d")

    logger.info(f"Bot calistirildi. TR saat: {tr_hour:02d}:{tr_minute:02d}")

    state = load_state()
    tweets = load_tweets()

    now_total_minutes = tr_hour * 60 + tr_minute

    # Eski format (hour) varsa yeni gune zorla
    if state.get("plan") and state["plan"] and "hour" in state["plan"][0]:
        state["today"] = None

    # Yeni gun mu? Plan yap.
    if state.get("today") != today:
        plan = make_daily_plan(tr_hour, tr_minute)
        state["today"] = today
        state["plan"] = [{"minute": m, "done": False} for m in plan]
        plan_str = [f"{m//60:02d}:{m%60:02d}" for m in plan]
        logger.info(f"Yeni gun plani: TR {plan_str}")
        save_state(state)

    # Planı kontrol et
    plan = state.get("plan", [])
    if not plan:
        logger.info("Bugun icin plan yok veya is saatleri bitti.")
        return

    # Saati gelmis ve atilmamis tweet var mi?
    for slot in plan:
        if slot["done"]:
            continue
        if now_total_minutes >= slot["minute"]:
            tweet = get_next_tweet(tweets, state)
            if tweet is None:
                logger.info(f"Tum tweetler atildi ({len(state['posted_ids'])}/{len(tweets)})")
                return

            planned_str = f"{slot['minute']//60:02d}:{slot['minute']%60:02d}"
            logger.info(f"TR {planned_str} planliydi, simdi {tr_hour:02d}:{tr_minute:02d}. Tweet atiliyor #{tweet['id']}")
            try:
                response = post_tweet(tweet["text"])
                state["posted_ids"].append(tweet["id"])
                slot["done"] = True
                save_state(state)
                logger.info(f"BASARILI! #{tweet['id']}: {tweet['text'][:60]}... | Twitter ID: {response.data['id']}")
                return  # Bir calismada 1 tweet at, diger saatine kalsin
            except Exception as e:
                logger.error(f"HATA: {e}")
                save_state(state)
                sys.exit(1)

    # Hepsi ya atildi ya da saati gelmedi
    pending = [s for s in plan if not s["done"]]
    if pending:
        pending_str = [f"{s['minute']//60:02d}:{s['minute']%60:02d}" for s in pending]
        logger.info(f"Bekleyen: TR {pending_str}")
    else:
        logger.info("Bugunun tweetleri tamamlandi.")


if __name__ == "__main__":
    main()
