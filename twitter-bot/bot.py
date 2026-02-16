"""
Corplynk Twitter Bot - Automated Tweet Poster
Posts chronological development milestones at random times during UTC+0 business hours.
Designed to run via Windows Task Scheduler.
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

# State file - tracks which tweets have been posted
STATE_FILE = BASE_DIR / "tweets_posted.json"
TWEETS_FILE = BASE_DIR / "tweets.json"


def load_state():
    """Load posted tweet IDs from state file."""
    if STATE_FILE.exists():
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {"posted_ids": [], "last_posted_at": None}


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


def is_business_hours_utc():
    """Check if current time is within UTC+0 business hours (09:00 - 18:00)."""
    now_utc = datetime.now(timezone.utc)
    return 9 <= now_utc.hour < 18


def main():
    """Main entry point."""
    logger.info("Bot started")

    # Check business hours (safety check - Task Scheduler should handle this)
    if not is_business_hours_utc():
        logger.info("Outside business hours (UTC+0 09:00-18:00). Skipping.")
        return

    # Load data
    tweets = load_tweets()
    state = load_state()

    # Get next tweet
    tweet = get_next_tweet(tweets, state)
    if tweet is None:
        logger.info("All tweets have been posted! Add more tweets to tweets.json")
        return

    # Post
    try:
        logger.info(f"Posting tweet #{tweet['id']}: {tweet['text'][:50]}...")
        response = post_tweet(tweet["text"])

        # Update state
        state["posted_ids"].append(tweet["id"])
        state["last_posted_at"] = datetime.now(timezone.utc).isoformat()
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
