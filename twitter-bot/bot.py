"""
Corplynk Twitter Bot - Automated Tweet Poster
Posts chronological development milestones during business hours.
Task Scheduler runs hourly with RandomDelay for natural timing.
Bot decides whether to tweet based on remaining quota and time.
NO SLEEPING - posts immediately when decided.
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

# Setup
BASE_DIR = Path(__file__).parent
load_dotenv(BASE_DIR / ".env")

# Logging
logging.basicConfig(
    filename=BASE_DIR / "bot.log",
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S"
)
logger = logging.getLogger(__name__)

# Also log to console
console = logging.StreamHandler()
console.setLevel(logging.INFO)
console.setFormatter(logging.Formatter("%(asctime)s [%(levelname)s] %(message)s"))
logger.addHandler(console)

# Files
STATE_FILE = BASE_DIR / "tweets_posted.json"
TWEETS_FILE = BASE_DIR / "tweets.json"

# Config
MAX_TWEETS_PER_DAY = 2
MIN_HOURS_BETWEEN_TWEETS = 2
BUSINESS_HOURS_UTC = (8, 19)  # 08:00-19:00 UTC = 11:00-22:00 TR


def load_state():
    if STATE_FILE.exists():
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"posted_ids": [], "last_posted_at": None, "daily_count": 0, "last_date": None}


def save_state(state):
    with open(STATE_FILE, "w", encoding="utf-8") as f:
        json.dump(state, f, indent=2, ensure_ascii=False)


def load_tweets():
    with open(TWEETS_FILE, "r", encoding="utf-8") as f:
        return json.load(f)


def get_next_tweet(tweets, state):
    posted_ids = set(state["posted_ids"])
    for tweet in tweets:
        if tweet["id"] not in posted_ids:
            return tweet
    return None


def post_tweet(text):
    api_key = os.getenv("API_KEY")
    api_secret = os.getenv("API_SECRET")
    access_token = os.getenv("ACCESS_TOKEN")
    access_token_secret = os.getenv("ACCESS_TOKEN_SECRET")

    if not all([api_key, api_secret, access_token, access_token_secret]):
        raise ValueError("Missing Twitter API credentials in .env file")

    client = tweepy.Client(
        consumer_key=api_key,
        consumer_secret=api_secret,
        access_token=access_token,
        access_token_secret=access_token_secret
    )

    response = client.create_tweet(text=text)
    return response


def should_tweet_now(state, now_utc):
    today = now_utc.strftime("%Y-%m-%d")

    # Reset daily count if new day
    if state.get("last_date") != today:
        state["daily_count"] = 0
        state["last_date"] = today
        logger.info(f"New day: {today}, daily count reset.")

    # Already hit daily max
    daily_count = state.get("daily_count", 0)
    if daily_count >= MAX_TWEETS_PER_DAY:
        logger.info(f"Daily limit reached ({daily_count}/{MAX_TWEETS_PER_DAY}). Skipping.")
        return False

    # Check minimum time between tweets
    last_posted = state.get("last_posted_at")
    if last_posted:
        try:
            last_dt = datetime.fromisoformat(last_posted)
            hours_since = (now_utc - last_dt).total_seconds() / 3600
            if hours_since < MIN_HOURS_BETWEEN_TWEETS:
                logger.info(f"Only {hours_since:.1f}h since last tweet (min {MIN_HOURS_BETWEEN_TWEETS}h). Skipping.")
                return False
        except (ValueError, TypeError):
            pass

    # Calculate probability
    current_hour = now_utc.hour
    remaining_hours = BUSINESS_HOURS_UTC[1] - current_hour
    remaining_tweets = MAX_TWEETS_PER_DAY - daily_count

    if remaining_hours <= 0:
        logger.info("No business hours remaining today.")
        return False

    # Must tweet now if running out of time
    if remaining_hours <= remaining_tweets:
        logger.info(f"Must tweet: {remaining_hours}h left, {remaining_tweets} tweets remaining.")
        return True

    # Random probability
    probability = remaining_tweets / remaining_hours
    roll = random.random()
    decision = roll < probability
    logger.info(f"Probability: {probability:.2f}, roll: {roll:.2f} -> {'TWEET' if decision else 'SKIP'}")
    return decision


def main():
    logger.info("=" * 40)
    logger.info("Bot started")

    now_utc = datetime.now(timezone.utc)
    logger.info(f"UTC time: {now_utc.strftime('%Y-%m-%d %H:%M:%S')}")

    # Check business hours
    if not (BUSINESS_HOURS_UTC[0] <= now_utc.hour < BUSINESS_HOURS_UTC[1]):
        logger.info(f"Outside business hours (UTC {BUSINESS_HOURS_UTC[0]}:00-{BUSINESS_HOURS_UTC[1]}:00). Current: {now_utc.hour}:00 UTC.")
        return

    # Load data
    tweets = load_tweets()
    state = load_state()

    logger.info(f"State: posted={len(state['posted_ids'])}/{len(tweets)}, daily={state.get('daily_count', 0)}/{MAX_TWEETS_PER_DAY}")

    # Get next tweet
    tweet = get_next_tweet(tweets, state)
    if tweet is None:
        logger.info("All tweets posted! Add more to tweets.json.")
        return

    # Decide
    if not should_tweet_now(state, now_utc):
        save_state(state)
        return

    # POST IMMEDIATELY - no sleeping, no waiting
    logger.info(f"Posting tweet #{tweet['id']}: {tweet['text'][:60]}...")

    try:
        response = post_tweet(tweet["text"])

        state["posted_ids"].append(tweet["id"])
        state["last_posted_at"] = now_utc.isoformat()
        state["daily_count"] = state.get("daily_count", 0) + 1
        state["last_date"] = now_utc.strftime("%Y-%m-%d")
        save_state(state)

        logger.info(f"SUCCESS! Tweet #{tweet['id']} posted. Response: {response.data}")

    except tweepy.TweepyException as e:
        logger.error(f"Twitter API error: {e}")
        save_state(state)
        sys.exit(1)
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        save_state(state)
        sys.exit(1)


if __name__ == "__main__":
    main()
