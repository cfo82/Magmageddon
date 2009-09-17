////////////////////////////////////////////////////////////////////
//
// Title.spa.h
//
// Auto-generated on Thursday, 17 September 2009 at 18:20:28
// XLAST project version 1.0.0.0
// SPA Compiler version 2.0.8276.0
//
////////////////////////////////////////////////////////////////////

#ifndef ___XBLA_PROJECTMAGMA_SPA_H__
#define ___XBLA_PROJECTMAGMA_SPA_H__

#ifdef __cplusplus
extern "C" {
#endif

//
// Title info
//

#define TITLEID__XBLA_PROJECTMAGMA                  0x11111111

//
// Context ids
//
// These values are passed as the dwContextId to XUserSetContext.
//


//
// Context values
//
// These values are passed as the dwContextValue to XUserSetContext.
//

// Values for X_CONTEXT_PRESENCE

#define CONTEXT_PRESENCE_PRESENCEMODE1              0

// Values for X_CONTEXT_GAME_MODE

#define CONTEXT_GAME_MODE_GAMEMODE1                 0

//
// Property ids
//
// These values are passed as the dwPropertyId value to XUserSetProperty
// and as the dwPropertyId value in the XUSER_PROPERTY structure.
//

#define PROPERTY_XNAVIRTUALTITLEIDPART1             0x10000001
#define PROPERTY_XNAVIRTUALTITLEIDPART2             0x10000002
#define PROPERTY_XNAVIRTUALTITLEIDPART3             0x10000007
#define PROPERTY_XNAVIRTUALTITLEIDPART4             0x10000008
#define PROPERTY_XNASESSIONISJOINABLE               0x10000009
#define PROPERTY_XNAALLOWHOSTMIGRATION              0x1000001A
#define PROPERTY_XNAFRAMEWORKVERSION                0x1000001B
#define PROPERTY_XNAHOSTGAMERTAGPART1               0x20000003
#define PROPERTY_XNAHOSTGAMERTAGPART2               0x20000004
#define PROPERTY_XNACUSTOMSESSIONPROPERTY1          0x2000000A
#define PROPERTY_XNACUSTOMSESSIONPROPERTY2          0x2000000B
#define PROPERTY_XNACUSTOMSESSIONPROPERTY3          0x2000000C
#define PROPERTY_XNACUSTOMSESSIONPROPERTY4          0x2000000D
#define PROPERTY_XNACUSTOMSESSIONPROPERTY5          0x2000000E
#define PROPERTY_XNACUSTOMSESSIONPROPERTY6          0x2000000F
#define PROPERTY_XNACUSTOMSESSIONPROPERTY7          0x20000010
#define PROPERTY_XNACUSTOMSESSIONPROPERTY8          0x20000011
#define PROPERTY_RATING                             0x2000001C

//
// Achievement ids
//
// These values are used in the dwAchievementId member of the
// XUSER_ACHIEVEMENT structure that is used with
// XUserWriteAchievements and XUserCreateAchievementEnumerator.
//

#define ACHIEVEMENT_ACHIEVEMENT01                   1
#define ACHIEVEMENT_ACHIEVEMENT02                   2
#define ACHIEVEMENT_ACHIEVEMENT03                   3
#define ACHIEVEMENT_ACHIEVEMENT04                   4
#define ACHIEVEMENT_ACHIEVEMENT05                   5
#define ACHIEVEMENT_ACHIEVEMENT06                   6
#define ACHIEVEMENT_ACHIEVEMENT07                   7
#define ACHIEVEMENT_ACHIEVEMENT08                   8
#define ACHIEVEMENT_ACHIEVEMENT09                   9
#define ACHIEVEMENT_ACHIEVEMENT10                   10
#define ACHIEVEMENT_ACHIEVEMENT11                   11
#define ACHIEVEMENT_ACHIEVEMENT12                   12

//
// Stats view ids
//
// These are used in the dwViewId member of the XUSER_STATS_SPEC structure
// passed to the XUserReadStats* and XUserCreateStatsEnumerator* functions.
//

// Skill leaderboards for ranked game modes

#define STATS_VIEW_SKILL_RANKED_GAMEMODE1           0xFFFF0000

// Skill leaderboards for unranked (standard) game modes

#define STATS_VIEW_SKILL_STANDARD_GAMEMODE1         0xFFFE0000

// Title defined leaderboards

#define STATS_VIEW_OVERALLLEADERBOARD               1

//
// Stats view column ids
//
// These ids are used to read columns of stats views.  They are specified in
// the rgwColumnIds array of the XUSER_STATS_SPEC structure.  Rank, rating
// and gamertag are not retrieved as custom columns and so are not included
// in the following definitions.  They can be retrieved from each row's
// header (e.g., pStatsResults->pViews[x].pRows[y].dwRank, etc.).
//

// Column ids for OVERALLLEADERBOARD


//
// Matchmaking queries
//
// These values are passed as the dwProcedureIndex parameter to
// XSessionSearch to indicate which matchmaking query to run.
//

#define SESSION_MATCH_QUERY_FINDSESSIONS            0

//
// Gamer pictures
//
// These ids are passed as the dwPictureId parameter to XUserAwardGamerTile.
//



#ifdef __cplusplus
}
#endif

#endif // ___XBLA_PROJECTMAGMA_SPA_H__


