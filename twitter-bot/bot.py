"""
Corplynk Twitter Bot - Automated Tweet Poster
Posts chronological development milestones at random times during UTC+0 business hours.
Task Scheduler runs every hour (12:00-21:00 TR / 09:00-18:00 UTC).
Bot decides randomly whether to tweet, targeting ~2 tweets/day.
"""

import json
import os
import sys
import random
import logging
import time
from datetime import datetime, timezone, timedelta
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

# State file - tracks which tweets have been posted
STATE_FILE = BASE_DIR / "tweets_posted.json"
TWEETS_FILE = BASE_DIR / "tweets.json"

# Config
MAX_TWEETS_PER_DAY = 2
MIN_HOURS_BETWEEN_TWEETS = 2
BUSINESS_HOURS_UTC = (9, 18)  # 09:00-18:00 UTC = 12:00-21:00 TR


def load_state():
    """Load posted tweet IDs from state file."""
    if STATE_FILE.exists():
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"posted_ids": [], "last_posted_at": None, "daily_count": 0, "last_date": None}


def save_state(state):
    """Save posted tweet IDs to state file."""
    with open(STATE_FILE, "w", encoding="utf-8") as f:
        json.dump(state, f, indent=2, ensure_ascii=False)


def load_tweets():
    """Load tweets from JSON file."""
    with open(TWEETS_FILE, "r", encoding="utf-8") as f:
        return json.load(f)


def get_next_tweet(tweets, state):
    """Get the next unposted tweet in chronological order."""
    posted_ids = set(state["posted_ids"])
    for tweet in tweets:
        if tweet["id"] not in posted_ids:
            return tweet
    return None  # All tweets posted


def post_tweet(text):
    """Post a tweet using Twitter API v2 via tweepy."""
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


def should_tweet_now(state):
    """Randomly decide whether to tweet this hour, considering daily limits and spacing."""
    now_utc = datetime.now(timezone.utc)
    today = now_utc.strftime("%Y-%m-%d")

    # Reset daily count if new day
    if state.get("last_date") != today:
        state["daily_count"] = 0
        state["last_date"] = today

    # Already hit daily max
    if state.get("daily_count", 0) >= MAX_TWEETS_PER_DAY:
        logger.info(f"Daily limit reached ({MAX_TWEETS_PER_DAY} tweets today). Skipping.")
        return False

    # Check minimum time between tweets
    if state.get("last_posted_at"):
        try:
            last_posted = datetime.fromisoformat(state["last_posted_at"])
            hours_since = (now_utc - last_posted).total_seconds() / 3600
            if hours_since < MIN_HOURS_BETWEEN_TWEETS:
                logger.info(f"Only {hours_since:.1f}h since last tweet (min {MIN_HOURS_BETWEEN_TWEETS}h). Skipping.")
                return False
        except (ValueError, TypeError):
            pass

    # Calculate probability based on remaining tweets and remaining hours
    current_hour = now_utc.hour
    remaining_hours = BUSINESS_HOURS_UTC[1] - current_hour
    remaining_tweets = MAX_TWEETS_PER_DAY - state.get("daily_count", 0)

    if remaining_hours <= 0:
        return False

    if remaining_hours <= remaining_tweets:
        # Running out of time, must tweet now
        logger.info(f"Only {remaining_hours}h left, {remaining_tweets} tweets remaining. Tweeting now.")
        return True

    # Random probability: spread tweets across remaining hours
    probability = remaining_tweets / remaining_hours
    roll = random.random()
    logger.info(f"Tweet probability: {probability:.2f}, rolled: {roll:.2f} (remaining: {remaining_tweets} tweets in {remaining_hours}h)")

    return roll < probability


def main():
    """Main entry point."""
    logger.info("Bot started")

    now_utc = datetime.now(timezone.utc)

    # Check business hours
    if not (BUSINESS_HOURS_UTC[0] <= now_utc.hour < BUSINESS_HOURS_UTC[1]):
        logger.info(f"Outside business hours (UTC {BUSINESS_HOURS_UTC[0]}:00-{BUSINESS_HOURS_UTC[1]}:00). Skipping.")
        return

    # Load data
    tweets = load_tweets()
    state = load_state()

    # Get next tweet
    tweet = get_next_tweet(tweets, state)
    if tweet is None:
        logger.info("All tweets have been posted! Add more tweets to tweets.json")
        return

    # Random decision: should we tweet now?
    if not should_tweet_now(state):
        save_state(state)  # Save updated daily_count/last_date
        return

    # Post (random delay handled by Task Scheduler's RandomDelay setting)
    try:
        logger.info(f"Posting tweet #{tweet['id']}: {tweet['text'][:50]}...")
        response = post_tweet(tweet["text"])

        # Update state
        state["posted_ids"].append(tweet["id"])
        state["last_posted_at"] = now_utc.isoformat()
        state["daily_count"] = state.get("daily_count", 0) + 1
        state["last_date"] = now_utc.strftime("%Y-%m-%d")
        save_state(state)

        logger.info(f"Tweet #{tweet['id']} posted successfully! Response: {response.data}")
        print(f"OK - Tweet #{tweet['id']} posted: {tweet['text'][:80]}...")

    except tweepy.TweepyException as e:
        logger.error(f"Twitter API error: {e}")
        print(f"ERROR - Twitter API: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        print(f"ERROR - {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
