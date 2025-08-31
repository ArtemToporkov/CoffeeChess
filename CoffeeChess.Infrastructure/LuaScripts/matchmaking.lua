local challengerPlayerId = ARGV[1]
local challengerPlayerRating = tonumber(ARGV[2])
local challengerMinRating = tonumber(ARGV[3])
local challengerMaxRating = tonumber(ARGV[4])
local challengerColorPreference = tonumber(ARGV[5])

local challengePoolKey = KEYS[1]

local potentialCandidates = redis.call('ZRANGEBYSCORE', challengePoolKey, challengerMinRating, challengerMaxRating, 'WITHSCORES')

if #potentialCandidates == 0 then
    return nil
end

local validCandidates = {}

for i = 1, #potentialCandidates, 2 do
    local candidateId = potentialCandidates[i]
    local candidateRating = tonumber(potentialCandidates[i+1])

    if candidateId ~= challengerPlayerId then
        local candidateDetailsKey = "challenge:" .. candidateId
        local candidateDetails = redis.call('HGETALL', candidateDetailsKey)

        if #candidateDetails > 0 then
            local candidateData = {}
            for j = 1, #candidateDetails, 2 do
                candidateData[candidateDetails[j]] = candidateDetails[j+1]
            end

            local candidateMinPref = tonumber(candidateData['minEloRatingPreference'])
            local candidateMaxPref = tonumber(candidateData['maxEloRatingPreference'])

            if challengerPlayerRating >= candidateMinPref and challengerPlayerRating <= candidateMaxPref then
                table.insert(validCandidates, {
                    id = candidateId,
                    rating = candidateRating,
                    ratingDiff = math.abs(challengerPlayerRating - candidateRating),
                    colorPreference = tonumber(candidateData['colorPreference']),
                    details = candidateData
                })
            end
        end
    end
end

if #validCandidates == 0 then
    return nil
end

table.sort(validCandidates, function(a, b)
    return a.ratingDiff < b.ratingDiff
end)

for _, candidate in ipairs(validCandidates) do
    local candidateColorPreference = candidate.colorPreference
    local colorMatch = false

    if challengerColorPreference == 2 or candidateColorPreference == 2 then
        colorMatch = true
    elseif (challengerColorPreference == 0 and candidateColorPreference == 1) or
            (challengerColorPreference == 1 and candidateColorPreference == 0) then
        colorMatch = true
    end

    if colorMatch then
        redis.call('ZREM', challengePoolKey, candidate.id)
        redis.call('DEL', "challenge:" .. candidate.id)

        local result = {}
        for key, value in pairs(candidate.details) do
            table.insert(result, key)
            table.insert(result, value)
        end
        return result
    end
end

return nil